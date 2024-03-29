# 미니 리듬 게임

* 달토끼
    - 지정된 리듬에따라 달토끼가 절구질을 하며 대기시 사용자가 터치로 절구질을 따라하게 함
* 흥부전 (진행중)
    - 리듬에 따라 톱질이 진행되면 맞춰서 터치
    - 일정부분 톱이 아래로 내려가면 박이 열림
* 떡 먹는 호랑이(해님달님) (진행중)
    - 날아오는 떡을 호랑이가 잡아서 먹음, 돌 등은 잡아서 버림
* 빨간 구두
* 돼지 삼형제
* 홍길동 (진행중)
    - 날아오는 화살을 쳐내거나 피함
    
    
---
** 10월 24일 **
1. 불필요한 코드 제거 및 정리
	- 키보드 입력 제거
	- 일부 분할된 Coroutine 영역 합침 
	- 게임 종료 체크 부분 위치 변경
	- 게임 순서 조정
2. 게임 로고 및 아이콘 적용
	- 일정시간 지속되는 로고 애니메이션의 경우 재생 완료 후 넘어가도록 함
3. 일시 정지 버튼 위치 조정
	- 일부 화면 비율에서 일시 정지 버튼이 게임 영역 밖에 표시되는 문제 수정
---
** 10월 23일 **
1. 빨간 구두
	- 게임 기초 구성 및 DB 설정
	- 이미지 및 애니메이션 적용
	- 비트 노트 적용(달토끼)
	- 생각을 보여주는 부분을 사라졌다 나타남을 이용해 보여줌
	- 터치 및 정답 체크 기능 처리
2. 게임 선택 화면
    - 게임 아이콘 적용
3. 게임 화면 크기 조정으로 일부 상수 값 조정
---
** 10월 20일 **
1. 돼지 삼형제
	- 유령 생성 위치 및 이동 속도 조정
2. 게임 로고 오류 수정
    - 화면보다 크게 설정되어 짤리는 문제 수정
---
** 10월 17일 **
1. 돼지 삼형제
    - 기본 구성 및 기초 설정
    - 유령 생성 및 이동 처리
    - 게임 DB 및 게임 선택 수정
    - 돼지 터치 관련 항목 변경
    - 늑대 상하 이동 처리
    - 정답 체크 부분 일부 수정
---
** 10월 16일 **
1. 게임 내 효과음 재생
    - 흥부전 : 박이 열릴 때 효과음 재생
    - 떡 먹는 호랑이 : 돌에 맞거나 쳐 낼 경우 효과음 재생
    - 게임 시작시 필요한 효과음 로드하도록 수정
2. 홍길동
    - 일부 UI 크기가 조정되지 않는 문제 수정
    - 배경음악이 다른 게임에 비해 음량이 큰 문제 수정
    - 화살 이동 관련 수정
---
** 10월 15일 **
1. 홍길동
    - 기본 구성 및 기초 설정
    - 화살 이동 및 발사 처리
    - 게임 DB 수정
2. 떡 먹는 호랑이(해님달님)
    - 게임 로고 설정
---
** 10월 14일 **
1. 게임 내 불필요한 코드 제거 및 정리
2. 다수 배경음악 및 비트 노트를 지원하도록 수정
---
** 10월 9일 **
1. 해님달님
    - 배경음악 및 비트 노트 수정 (달토끼용)
    - 떡 및 돌 이동 개선
---
** 10월 7일 **
1. 해님달님
    - 애니메이션 적용
    - 떡(돌) 이동 방식 변경 처리
2. 게임 오류 수정
---
** 10월 3일 **
1. 게임별 정답 체크 부분 수정
    - 시간으로 체크하도록 수정
2. 마우스 클릭으로 게임이 진행되도록 수정
---
** 10월 1일 **
1. 해님달님
    - 호랑이 동작 변경에 따른 수정
        - 점프에서 손을 휘두르는 애니메이션으로 변경
        - 정답 처리 방식 변경 및 수정 작업중
---
** 9월 30일 **
1. 해님달님
    - UI 적용 및 게임 공용 부분 수정
---
** 9월 29일 **
1. 달토끼 정답 체크 부분 변경
    - 비트 노트 수정
    - 불필요한 부분 제거 및 코드 변경
    - 정답 인정 영역 축소
 2. 일시정지, 종료 UI
     - 버튼 이미지 변경
     - 터치 효과 적용
---
** 9월 28일 **
1. 페이스북 연동 코드 제거
2. 흥부전
    - 게임 로고 애니메이션 누락 수정
    - 박 및 톱의 위치 및 크기 조정
---
** 9월 27일 **
1.  게임 공통
    - 게임 플레이시 터치 시점 변경
    - 게임 UI 수정 - 일시 정지 버튼만 남기고 모두 제거
    - 누락된 애니메이션 수정
    - 해당 게임 시작시 로고 애니메이션 재생
    - 시간에 따른 게임 종료 제거
2. 흥부전
    - 톱질시 좌우 이동만 하도록 함
    - 일정 beat에 따라 터치하며, 이 결과에 따라 특정 부분에서 애니메이션 재생하도록 함
3. 게임 선택 화면
    - 게임 시작, 닫기 버튼 선택시 효과 적용