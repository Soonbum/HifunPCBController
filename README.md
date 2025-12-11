# HifunPCBController

HIFUN PCB 보드 컨트롤러입니다.

## GPIO핀에 대한 정보

1. GPIO핀 (https://armyost.tistory.com/347, https://fishpoint.tistory.com/7687)
<img width="1032" height="1375" alt="image" src="https://github.com/user-attachments/assets/3ae7e3dd-ced8-4117-8bd1-916645bce548" />
<img width="838" height="554" alt="image" src="https://github.com/user-attachments/assets/92fa7200-48b1-4f40-b90c-0e54b3de3cdf" />

2. pinout 명령어 (https://m.blog.naver.com/rlrkcka/223515961056)
<img width="442" height="955" alt="image" src="https://github.com/user-attachments/assets/4ed59d23-2044-4774-ba34-0fc68758d418" />
<img width="800" height="1329" alt="image" src="https://github.com/user-attachments/assets/5a402df4-2aa2-453c-9db0-be88d7d574b0" />

## 본 프로그램을 작동하기 위한 결선 방식

* 먼저 Tera Term 또는 PuTTY로 테스트
  - 보드로 보낼 때 줄바꿈 문자는 "CR+LF"이어야 함

* HIFUN PCB 보드 -- USB-TTL 컨버터
  - 연결 조건: 9600/8/1/NoneParity/NoFlowControl
  - 핀 연결 (39번 -- GND, 32번 -- RXD, 33번 -- TXD)

![20251211_123058](https://github.com/user-attachments/assets/452739a4-3e71-4384-8486-cff623d7308b)
![20251211_123109](https://github.com/user-attachments/assets/8ed996d8-96d6-4b74-aaac-3a3264ae7006)
