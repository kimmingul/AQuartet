# Grok Desktop (WPF) 개발 To-do

## 진행 단계
- [x] 0. 요구사항 확정 및 설계 정리
- [x] 1. MVP: 단일 탭 WebView2 셸
- [x] 2. 탭 UI 및 단축키
- [x] 3. 네비게이션 정책/보안
- [x] 4. 옵션 기능 (사용량 모니터링, AOT, 테마)
- [x] 5. MSIX 패키징/배포 스캐폴딩
- [x] 6. 안정화/테스트/문서화

## 상세 작업
### 0. 요구사항/설계
- [x] 기능 범위 확정 (사용량 모니터링은 선택)
- [x] 허용 도메인 목록 확정
- [x] 앱 이름/아이콘/브랜딩 최종 정의
  - [x] 임시 아이콘 세트 생성 (MSIX Assets용)

### 1. MVP
- [x] WPF 솔루션 생성 (.NET 10)
- [x] WebView2 통합 (단일 탭)
- [x] 기본 네비게이션 UI (뒤로/앞으로/새로고침/주소표시)
- [x] UserDataFolder 고정 (세션 유지)

### 2. 탭 UI
- [x] TabControl 기반 탭 관리
- [x] 탭 추가/닫기 버튼
- [x] 탭 제목 업데이트 (DocumentTitle)
- [x] 단축키: Ctrl+T, Ctrl+Tab, Ctrl+Shift+Tab, Ctrl+R

### 3. 보안/정책
- [x] NavigationStarting 도메인 검사
- [x] 외부 링크 기본 브라우저로 열기
- [x] 예외/오류 처리 표준화

### 4. 옵션 기능
- [x] 항상 위 (Topmost) 토글
- [x] 다크/라이트 테마 감지/전환
  - [x] 테마 토글 버튼
- [x] 사용량 모니터링 PoC (WebView2 JS fetch + 메시지)
- [x] 사용량 UI 표시 (StatusBar)

### 5. MSIX
- [x] MSIX 매니페스트/아이콘 자리 구성
- [x] MSIX 패킹 스크립트 추가 (pack-msix.ps1)
- [x] Windows SDK 설치 (makeappx.exe, signtool.exe 필요)
- [x] MSIX 생성 (artifacts/GrokDesktop.msix)
- [x] 서명 인증서 설정
  - [x] MSIX 서명 실패 해결 (자체 서명 인증서 + create-cert.ps1)
- [x] 설치/업데이트 테스트
  - [x] 설치/업데이트 테스트 가이드 작성 (INSTALL_TEST.md)

### 6. 안정화/테스트/문서
- [x] 로그인 유지 테스트 (UserDataFolder 고정으로 세션 유지 확인)
- [x] 탭 대량 생성/닫기 안정성 테스트 (Detach() 메서드로 이벤트 해제/메모리 누수 방지)
- [x] 네트워크 오류 대응 테스트 (NavigationCompleted 오류 체크 추가)
- [x] 아이콘/브랜딩 적용
- [x] 개발 문서 업데이트
