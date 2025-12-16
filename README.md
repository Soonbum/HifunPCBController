# HifunPCBController

HIFUN PCB 보드 컨트롤러입니다.


## GPIO핀에 대한 정보

1. GPIO핀 (https://armyost.tistory.com/347, https://fishpoint.tistory.com/7687)
<img width="838" height="554" alt="image" src="https://github.com/user-attachments/assets/92fa7200-48b1-4f40-b90c-0e54b3de3cdf" />
<img width="507" height="504" alt="image" src="https://github.com/user-attachments/assets/311e4e68-28b0-4f13-936e-214321bb588e" />

2. pinout 명령어 (https://m.blog.naver.com/rlrkcka/223515961056)
<img width="442" height="955" alt="image" src="https://github.com/user-attachments/assets/4ed59d23-2044-4774-ba34-0fc68758d418" />
<img width="800" height="1329" alt="image" src="https://github.com/user-attachments/assets/5a402df4-2aa2-453c-9db0-be88d7d574b0" />

3. 라즈베리파이 보드(좌측)와 HIFUN 보드(우측) 연결 방식
<img width="1716" height="1226" alt="보드 연결 방법" src="https://github.com/user-attachments/assets/65e73e85-d0f4-44c9-af7b-90c9ad62e293" />
   * 참고로 HIFUN 보드는 라즈베리파이 보드가 아니라 별도의 맞춤 제작 보드입니다.
   * 라즈베리파이 보드 측: 24핀 (GPIO 8) = TX, 21핀 (GPIO 9) = RX (안쪽부터 1번)
   * HIFUN 보드 측: 24번 홀 = RX, 21번 홀 = TX (바깥쪽부터 1번)



## 본 프로그램을 작동하기 위한 결선 방식

* 먼저 Tera Term 또는 PuTTY로 테스트
  - 보드로 보낼 때 줄바꿈 문자는 "CR+LF"이어야 함

* HIFUN PCB 보드 -- USB-TTL 컨버터
  - USB-TTL 컨버터는 5V 신호 보내야 함
  - 연결 조건: BaudRate:9600 / DataBit:7 / StopBit:2 / Parity:None / FlowControl:No
  - 핀 연결 (39번 -- GND, 32번 GPIO 12 (PWM0) -- RXD, 33번 GPIO 13 (PWM1) -- TXD) ??? (조사중)
  - 선 간의 노이즈를 줄여야 함 (차폐 또는 Twisted)

* USB-TTL 컨버터 선
  ![20251211_123109](https://github.com/user-attachments/assets/8ed996d8-96d6-4b74-aaac-3a3264ae7006)

* HIFUN PCB 보드의 선
  (조사중)
