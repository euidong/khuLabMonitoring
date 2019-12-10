# khu-lab-monitoring

### Member
- JHyunB
- euidong

### Goal 
- set PC ON / OFF
  - PC ON은 Port forwarding 문제로 학교 내 보안 문제로 구현 제한.
- catch PC Suspend mode
- add tray bar
- get data CPU/RAM/HDD from PC
- implement memo stage

### Server 
- UDP 통신을 보내는 대상
- GUI 구현(tray, mainView, subView, comInfo)
  - tray : 상태바 아이콘
  - mainWindow : 전체 랩실을 보여줌.
  - subWindow : 랩실 하나의 모습을 보여줌.
  - comInfo : pc하나의 상태를 보여줌.
- memo 창 구현.(컴퓨터와 lab실의 정보를 저장하기 위한 memo 창)

### Client 
- UDP 통신을 받을 준비를 계속하고 있는다.
- pc의 정보를 보낼 준비를 하고 있음.
- 요청이 들어오면 바로 전송.
- suspend mode를 catch하고 server에게 signal.
