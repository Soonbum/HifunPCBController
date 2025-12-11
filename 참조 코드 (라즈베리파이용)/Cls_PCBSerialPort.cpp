#include "Cls_PCBSerialPort.h"
#include <QFile>
#include <QThread>
#include <QApplication>

#include "mainwindow.h"
#include "Variable_Mgr/Cls_GlobalVariable.h"

Cls_PCBSerialPort::Cls_PCBSerialPort(MainWindow *mainWindow, QObject *parent) : QObject(parent), pMainWindow(mainWindow)
{
    pSerialPort = new QSerialPort(this);
    pGVariable = pMainWindow->pGVariable;
}

Cls_PCBSerialPort::~Cls_PCBSerialPort()
{
    closeSerialPort();
}

void Cls_PCBSerialPort::initSerialPort()
{
    QString sUSBPath;
    qint32 baudRate;

    emit pMainWindow->writeSystemLog("System", "Initializing Serial Connection with " + pGVariable->sControlBoard, false);

    if (pGVariable->sControlBoard == "HIFUN Board")
    {
        sUSBPath = "/dev/ttyAMA4";
        baudRate = QSerialPort::Baud9600;
    }
    else
    {
        sUSBPath = "/dev/ttyUSB_PCB";
        baudRate = QSerialPort::Baud9600;

        if (QFile::exists("/var/lock/LCK..ttyUSB_PCB"))
        {
            QFile::remove("/var/lock/LCK..ttyUSB_PCB");
        }
    }

    if (!pSerialPort)
    {
        pSerialPort = new QSerialPort(this);
    }
    else
    {
        if (pSerialPort->isOpen())
        {
            pSerialPort->close();
        }
        pSerialPort->clearError();
    }

    pSerialPort->setPortName(sUSBPath);
    pSerialPort->setBaudRate(baudRate);
    pSerialPort->setDataBits(QSerialPort::Data8);
    pSerialPort->setParity(QSerialPort::NoParity);
    pSerialPort->setStopBits(QSerialPort::OneStop);
    pSerialPort->setFlowControl(QSerialPort::NoFlowControl);

    if (pSerialPort->open(QIODevice::ReadWrite))
    {
        emit pMainWindow->writeSystemLog("System", "PCB Connected", false);
    }
    else
    {
        emit pMainWindow->writeSystemLog("Fatal", "PCB Not Connected : " + pSerialPort->errorString(), false);
        emit pMainWindow->sigSetGeneralPopUp("Connect failed : " + pSerialPort->errorString() + "\nPCB Reset Required");
    }

    if (pGVariable->sControlBoard != "HIFUN Board")
    {
        if (QFile::exists("/var/lock/LCK..ttyUSB_PCB"))
        {
            QFile::remove("/var/lock/LCK..ttyUSB_PCB");
        }
    }
}

void Cls_PCBSerialPort::closeSerialPort()
{
    if (pSerialPort && pSerialPort->isOpen())
    {
        pSerialPort->close();
    }
}

void Cls_PCBSerialPort::reconnectSerialPort()
{
    emit writeSystemLog("System", "Reconnecting serial port", false);
    closeSerialPort();
    initSerialPort();
}

bool Cls_PCBSerialPort::isOpen() const
{
#ifdef QT_NO_DEBUG
    return pSerialPort->isOpen();
#else
    return false;
#endif
}

QString convertSerialPortErrorToString(QSerialPort::SerialPortError error)
{
    switch (error)
    {
        case QSerialPort::DeviceNotFoundError: return "Device Not Found";
        case QSerialPort::PermissionError: return "Permission Denied";
        case QSerialPort::OpenError: return "Failed to Open Port";
        default: return "Unknown Error";
    }
}

QByteArray Cls_PCBSerialPort::writeAllCommand()
{
    if (!pSerialPort) { return ""; }

    QString sPrinterModel = pGVariable->sPrinterName;
    QString sWritingCommand;
    unsigned long ulPinCommand = 0;
    int iReconnectTimer = 0;
    QByteArray baReadDataValue;

    static int callCount = 0;
    callCount++;

    Q_UNUSED(iReconnectTimer);

    // Command : Build Platform Position Target
    sWritingCommand = "7E01" + pGVariable->convertPositionValueForMotorDriver(pGVariable->dTargetMotorPosition) + "0,";

    // Command : Check Build Platform Current Position
    sWritingCommand += "7E02,";

    // Command : Motor Position Reset - 7E03

    // Command : Set Build Platform Speed
    sWritingCommand += "7E04" + pGVariable->convertSpeedValueForMotorDriver(pGVariable->dTargetMotorSpeed, pGVariable->setMaxSpeed) + ",";

    // pinCommand Set
    ulPinCommand |= 1 << (pGVariable->iTargetBladePosition + 18);
    ulPinCommand |= 1 << (pGVariable->setPTCOn ? 17 : 16);
    ulPinCommand |= 1 << (pGVariable->setFanOn ? 14 : 13);
    ulPinCommand |= 1 << 15; // Temperature
    ulPinCommand |= 1 << (pGVariable->setButtonLEDOn ? 12 : 11);
    ulPinCommand |= 1 << pGVariable->signalTowerColorCode;

    if (pGVariable->sPrinterName == "IMC-D")
    {
        ulPinCommand |= 1 << pGVariable->iTargetBladePosition;
    }
    else
    {
        ulPinCommand |= 1 << pGVariable->resinPumpStatusCode;
    }

    QString sHexCommand = QString::number(ulPinCommand, 16).toUpper();
    int iWidth = (sHexCommand.length() <= 5) ? 5 : 6;

    sWritingCommand += "7E05" + sHexCommand.rightJustified(iWidth, QLatin1Char('0')) + ",";

    // Command : LCD-LED PWM
    sWritingCommand += "7E06" + QString::number(pGVariable->iTargetPWM).rightJustified(3, '0') + ",";

    // Command : Buzzer
    if (pGVariable->setShutdownBuzzer)
    {
        sWritingCommand += "7E07,";
    }

    if (pGVariable->setPrintFinishBuzzer)
    {
        sWritingCommand += "7E08,";
    }

    if (pGVariable->sControlBoard == "HIFUN Board")
    {
        sWritingCommand += "\r\n";
    }

#ifdef QT_NO_DEBUG
    try
    {
        if (!pSerialPort->isOpen())
        {
            emit pMainWindow->writeSystemLog("Fatal", "Serial port not opened but tried to WriteAllCommand", false);
            return "";
        }
        pSerialPort->clear();

        pSerialPort->write(sWritingCommand.toLocal8Bit());
        pSerialPort->waitForBytesWritten();
        if ((callCount % 10 == 0) && pGVariable->enableDebugLog)
        {
            qDebug() << "Write-All-Command (Write): " << sWritingCommand;
        }
    }
    catch (...)
    {
        emit pMainWindow->writeSystemLog("Fatal", "Couldn't Write All Command", false);
        return "";
    }

    try
    {
        while (true)
        {
            if (pSerialPort->waitForReadyRead(1000))
            {
                iReconnectTimer = 0;
                if (pSerialPort->canReadLine())
                {
                    QByteArray line = pSerialPort->readLine();
                    baReadDataValue.append(line);
                }

                if (baReadDataValue.contains("\r\n") || baReadDataValue.contains("\n") || baReadDataValue.contains("\r"))
                {
                    if ((callCount % 10 == 0) && pGVariable->enableDebugLog)
                    {
                        qDebug() << "Write-All-Command (Read): " << baReadDataValue;
                        emit pMainWindow->sigSetCurrentSerialData(sWritingCommand, baReadDataValue);
                    }
                    break;
                }
            }
            else
            {
                emit pMainWindow->writeSystemLog("Fatal", "Read All Time Out(" + QString::number(iReconnectTimer) + ") with : " + baReadDataValue, false);

                pSerialPort->readAll();
                pSerialPort->clear();
                baReadDataValue.clear();

                QThread::msleep(500);

                iReconnectTimer++;

                if (iReconnectTimer > 10)
                {
                    iReconnectTimer = 0;
                    return "";
                }
            }
        }
    }
    catch (...)
    {
        emit pMainWindow->writeSystemLog("Fatal", "Read Command Error", false);
    }
#endif
    return baReadDataValue;
}

void Cls_PCBSerialPort::writeCommand(QString sWritingCommand)
{
    if (!pSerialPort) { return; }

    QByteArray readDataValue = "";

    pSerialPort->clear();

    if (!pSerialPort->isOpen())
    {
        emit pMainWindow->writeSystemLog("Fatal", "Serial port not opened but tried to WriteCommand", false);
    }

    try
    {
        if(pGVariable->sControlBoard == "HIFUN Board")
        {
            sWritingCommand += "\r\n";
        }

        pSerialPort->write(sWritingCommand.toLocal8Bit());

        if (pSerialPort->waitForBytesWritten())
        {
            if (pGVariable->enableDebugLog)
            {
                qDebug() << "Single Command Written : " << sWritingCommand;
            }
            emit pMainWindow->sigSetCurrentSerialData(sWritingCommand,"");
            QApplication::processEvents();
        }
    }
    catch (...)
    {
        emit pMainWindow->writeSystemLog("Fatal", "writeCommand Error", false);
    }
}

QByteArray Cls_PCBSerialPort::waitForReturn(const QString &sWrittenCommand)
{
    if (!pSerialPort) { return ""; }

    QByteArray returnData = "";
    int iReconnectTimer = 0;
    QByteArray baReadDataValue;

    Q_UNUSED(sWrittenCommand);

    try
    {
        while (true)
        {
            if (pSerialPort->waitForReadyRead(60000))
            {
                iReconnectTimer = 0;
                if (pSerialPort->canReadLine())
                {
                    QByteArray line = pSerialPort->readLine();
                    baReadDataValue.append(line);
                }

                if (baReadDataValue.contains("\r\n") || baReadDataValue.contains("\n") || baReadDataValue.contains("\r"))
                {
                    returnData = baReadDataValue;

                    if (pGVariable->enableDebugLog)
                    {
                        qDebug() << "Returned Data : " << returnData;
                    }

                    break;
                }
            }
            else
            {
                emit pMainWindow->writeSystemLog("Fatal", "Read All Time Out(" + QString::number(iReconnectTimer) + ") with : " + baReadDataValue, false);
                emit pMainWindow->writeSystemLog("Fatal",convertSerialPortErrorToString(pSerialPort->error()),true);
                pSerialPort->readAll();
                pSerialPort->clear();
                baReadDataValue.clear();

                QThread::msleep(500);

                iReconnectTimer++;

                if (iReconnectTimer > 10)
                {
                    iReconnectTimer = 0;
                    return "";
                }
            }
        }
    }
    catch (...)
    {
        emit pMainWindow->writeSystemLog("Fatal", "Read Command Error", false);
        return "";
    }

    pSerialPort->readAll();
    pSerialPort->clear();
    baReadDataValue.clear();
    return returnData;
}
