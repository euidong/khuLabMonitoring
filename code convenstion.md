- variable - camelCase
- const variable - 모두 대문자로 작성 ( EX. PORT )
- method - PascalCase
- class - PascalCase
- file명 - PascalCase
- indent = 2
- brace
    ```csharp
    FunctionName() {
    	// 내용
    }	
    ```
- Remark(주석)
  ```csharp
  /* 
    제목
    콘텐츠1
    콘텐츠2
  */

  // 주석 내용
  int variable;

  // 주석 내용
  FunctionName() {
  }
  ```
- 작명 센스
  ```markdown
  # 같은 기능끼리는 같은 언어를 사용한다. (비슷한 의미의 단어를 혼용하지 말 것)

  ### 기본
  - variable : 명사형
    * boolean 예외없음 (isXXX 는 함수에만 쓸 수 있음)
  - function : 동사형 (무조건 동사가 먼저 온다.)
  - class명 : 수행자 또는 사물의 이름으로 한다.
  수행지라는 느낌이 올 수 있도록 한다.
  ( ex. management => manager )

  ### 더하는 구분자
  - add : 두 number를 더하는 경우
  - insert : 목록에 추가할 때
  - concat : 두 string을 더하는 경우
  - append : 두 배열을 더하는 경우
  - push : 배열 맨 뒤에 넣는 경우

  ### 빼는 구분자

  - pop : 배열 맨 앞에 넣는 경우

  ### Create 구분자
  - create : 자기 스스로 데이터를 생성할 때
  - post : 외부에 데이터를 생성할 때

  ### Read 구분자
  - get : 자기가 갖고 있는 것을 불러올 때
  - load : 외부에 있는 데이터를 불러올 때

  ### Update 구분자
  - update : 자기 스스로 데이터를 수정할 때
  - put : 외부에 있는 데이터를 수정할 때 (한 번에 여러 개)
  - patch : 외부에 있는 데이터를 수정할 때 (한 번에 하나)

  ### Delete 구분자
  - remove : 자기 스스로 데이터를 삭제할 때
  - delete : 외부에 있는 데이터를 삭제할 때-

  ### 관리자 구분자
  - manager : 관리자는 manager를 사용한다. (controller X)

  ### 숫자와 관련된 이름
  - count : 개수
  - index : 0부터 시작하는 인덱스
  - num : 그 외 번호, 숫자 등
  ```
