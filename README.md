# khu-lab-monitoring

### Member
- JHyunB : (주)로직 설계 및 디자인 (부) 통신
- euidong : (주)통신 및 로직 설계 (부) 디자인

### Goal 
- 교내 pc 제어 프로그램 
  - set PC ON / OFF
    - PC ON은 학교 내 보안 문제로 Port forwarding 불가능으로 구현 제한.
  - catch PC Suspend mode
  - add tray bar
  - get data CPU/RAM/HDD from PC
  - implement memo stage

### Server 
- UDP 통신을 보내는 대상
- GUI 구현(tray, mainView, subView, comInfo)
  - tray : 상태바 아이콘
  <img src="./img/tray.png">
  
  - mainWindow : 전체 랩실을 보여줌.
  <img src="./img/mainWindow.png">
  
  - subWindow : 랩실 하나의 모습을 보여줌.<br>
  memo 창 구현.(컴퓨터와 lab실의 정보를 저장하기 위한 memo 창)
  <img src="./img/subWindow.png">
  
  - comInfo : pc하나의 상태를 보여줌.
  <img src="./img/comInfo.png">

### Client 
- UDP 통신을 받을 준비를 계속하고 있는다.
- pc의 정보를 보낼 준비를 하고 있음.
- 요청이 들어오면 바로 전송.
- suspend mode를 catch하고 server에게 signal.
