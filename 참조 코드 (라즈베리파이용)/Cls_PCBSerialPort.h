#ifndef CLS_PCBSERIAL_H
#define CLS_PCBSERIAL_H

#include <QObject>
#include <QSerialPort>

class MainWindow;
class Cls_GlobalVariable;

class Cls_PCBSerialPort : public QObject
{
    Q_OBJECT

public:
    explicit Cls_PCBSerialPort(MainWindow *mainWindow, QObject *parent = nullptr);
    ~Cls_PCBSerialPort();

    bool isOpen() const;

public slots:
    void initSerialPort();
    void reconnectSerialPort();
    void closeSerialPort();

    QByteArray writeAllCommand();
    void writeCommand(QString sWritingCommand);
    QByteArray waitForReturn(const QString &sWrittenCommand);

signals:
    void writeSystemLog(const QString &level, const QString &message, bool record = true);
    void sigSetCurrentSerialData(const QString &written, const QByteArray &received);

private:
    QSerialPort *pSerialPort;
    MainWindow *pMainWindow;
    Cls_GlobalVariable *pGVariable;
};

#endif // CLS_PCBSERIAL_H
