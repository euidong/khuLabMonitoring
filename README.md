# khu-lab-monitoring

<div align="center">

[![CORP](https://img.shields.io/badge/KHU-GURU-orange)](http://swedu.khu.ac.kr/html_2018/)
[![LANGUAGE](https://img.shields.io/badge/c%23-8.0-yellowgreen)](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-8)
[![FRAMEWORK](https://img.shields.io/badge/.NET%20Core-3.0-ff69b4)](https://dotnet.microsoft.com/download/dotnet-core/3.0)
[![IDE](https://img.shields.io/badge/VS-2019-green)](https://visualstudio.microsoft.com/vs/)
[![LICENSE](https://img.shields.io/badge/License-MIT-blueviolet)](https://ko.wikipedia.org/wiki/MIT_%ED%97%88%EA%B0%80%EC%84%9C)

</div>

<p align="center">
<img src='https://blog.kakaocdn.net/dn/bjsDsi/btqxXJM3JKe/WAK7xHbOm7kxyVqRIvoOaK/img.jpg' width="500px">
</p>

### Member
- JHyunB : (주)로직 설계 및 디자인 (부) 통신
- euidong : (주)통신 및 로직 설계 (부) 디자인
- wjlee0908 : 유지 보수 시스템 ui 변경 및 리팩토링 

### Goal 
- 교내 pc 제어 프로그램 
  - set PC ON / OFF
    - PC ON은 학교 내 보안 문제로 Port forwarding 불가능으로 구현 제한.
  - catch PC Suspend mode
  - add tray bar
  - get data CPU/RAM/HDD from PC
  - implement memo stage
  
  2차 유지보수 사항
  - server 불안정 버그 수정
    - 갑작스러운 연결 종료 이슈 확인 및 수정
    - 전체 서버 코드의 비동기 작업 부적절한 작동 확인 및 수정
  - coding convension 통일을 통해서 코드 가독성 강화   

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
