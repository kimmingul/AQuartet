# A-Quartet 개발 문서

이 문서는 Windows 전용 WPF + WebView2 기반 A-Quartet 앱 개발을 위한 설계와 구현 가이드를 제공합니다.

## 1. 목표 범위

- Windows 전용 데스크톱 앱 (WPF)
- 멀티 AI 서비스 통합: Grok, Claude, ChatGPT, Gemini, Perplexity, Copilot
- VS Code 스타일 사이드바 + 서비스별 탭 UI
- MSIX 배포

## 2. 기술 스택
- .NET 10 (WPF)
- WebView2 (Microsoft.Web.WebView2)
- MVVM (CommunityToolkit.Mvvm)
- WPF-UI 4.2.0 (Windows 11 Fluent Design)
- MSIX 패키징

## 3. 현재 구현 상태

- 멀티 AI 서비스 지원 (Grok, Claude, ChatGPT, Gemini, Perplexity, Copilot)
- VS Code 스타일 아이콘 사이드바 (서비스 전환)
- 서비스당 최대 1탭 (사이드바 클릭 시 기존 탭 전환 또는 새 탭 생성)
- 탭 전환 시 WebView2 상태 유지 (페이지 새로고침 없음)
- 탭 드래그 재정렬
- 탭 닫기 (✕ 버튼)
- Windows 11 Fluent Design (WPF-UI: FluentWindow, Mica 배경, Fluent 컨트롤)
- 서비스별 도메인 제한 (허용 도메인 + OAuth 로그인 도메인)
- 사이드바 하단: 새로고침(↻), 테마 전환(🌙/☀️), Always On Top(📌) 아이콘 버튼
- 다크/라이트/고대비 테마 전환 (WPF-UI ApplicationThemeManager)
- 하단 상태바에 현재 URL 표시
- MSIX 패키징 스크립트/매니페스트
- 단일 파일 배포 (framework-dependent)
- 키보드 단축키: Ctrl+Tab(다음 탭), Ctrl+Shift+Tab(이전 탭), Ctrl+R(새로고침)

## 4. 프로젝트 구조

```
migration/
├── DEVELOPMENT.md
├── TODO.md
├── AQuartet.slnx
├── packaging/
│   ├── AppxManifest.xml
│   ├── pack-msix.ps1
│   ├── sign-msix.ps1
│   ├── create-cert.ps1
│   ├── generate-icons.ps1
│   └── Assets/ (아이콘)
└── src/
    ├── AQuartet.App/ (WPF 앱)
    │   ├── AQuartet.App.csproj
    │   ├── App.xaml / App.xaml.cs
    │   ├── MainWindow.xaml / MainWindow.xaml.cs
    │   ├── AssemblyInfo.cs
    │   ├── app.ico
    │   ├── Assets/Logos/ (서비스 로고 PNG)
    │   │   └── download-logos.ps1
    │   ├── ViewModels/
    │   │   ├── MainViewModel.cs
    │   │   └── TabViewModel.cs
    │   └── Views/
    │       ├── TabView.xaml
    │       └── TabView.xaml.cs
    └── AQuartet.Core/ (공통 로직)
        ├── AQuartet.Core.csproj
        ├── AiService.cs
        ├── AiServiceRegistry.cs
        ├── NavigationPolicy.cs
        └── AppPaths.cs
```

## 5. 핵심 컴포넌트

### 5.1 UI 레이아웃

```text
┌─────────────────────────────────────────────────────────────┐
│  A-Quartet                                     ─  □  ✕      │
├────┬────────────────────────────────────────────────────────┤
│ G  │ [Grok ✕] [Claude ✕] [Gemini ✕]                        │
│ C  ├────────────────────────────────────────────────────────┤
│ GP │                                                        │
│ Gm │        WebView2 Content                                │
│ P  │        (탭 전환 시 상태 유지)                            │
│ Co │                                                        │
│────│                                                        │
│ ↻  │                                                        │
│ 🌙 │                                                        │
│ 📌 │                                                        │
├────┴────────────────────────────────────────────────────────┤
│ https://grok.com/                                           │
└─────────────────────────────────────────────────────────────┘
```

### 5.2 컴포넌트 설명

- **MainWindow** (FluentWindow): 사이드바 + TitleBar + 탭 헤더(ListBox) + 콘텐츠(ItemsControl) + StatusBar
- **MainViewModel**: 서비스 선택, 탭 관리(서비스당 1탭), 테마 전환, 탭 이동
- **TabViewModel**: WebView2 네비게이션/상태, AiService 연결, IsSelected, Detach() 리소스 정리
- **TabView**: WebView2 호스팅 (각 탭이 독립 WebView2 인스턴스 보유)
- **AiService** (Core): AI 서비스 정의 (이름, URL, 허용 도메인, 로고 리소스)
- **AiServiceRegistry** (Core): 전체 서비스 목록 관리
- **NavigationPolicy** (Core): 서비스별 도메인 허용 정책
- **AppPaths** (Core): WebView2 UserDataFolder 경로 (%LOCALAPPDATA%\AQuartet\WebView2)
- **App.xaml.cs**: 전역 예외 처리, ApplicationThemeManager 테마 관리

## 6. WebView2 설계

### 6.1 프로필/세션 유지
- UserDataFolder를 고정하여 로그인/쿠키 유지
- 경로: %LOCALAPPDATA%\AQuartet\WebView2

### 6.2 탭 상태 보존
- 각 탭이 독립 WebView2 인스턴스 보유
- TabControl 대신 ItemsControl(Grid 패널) + Visibility 바인딩으로 구현
- 탭 전환 시 WebView2를 새로 로드하지 않음 (IsSelected → Visible/Collapsed)

### 6.3 도메인 제한
- NavigationStarting 이벤트에서 URL 검사
- 서비스별 허용 도메인 + OAuth 도메인 (Google, Apple, Microsoft)
- 허용 외는 기본 브라우저로 열기

## 7. 멀티 AI 서비스

### 7.1 서비스 정의

| ID | 이름 | URL | 허용 도메인 |
|----|------|-----|-----------|
| grok | Grok | grok.com | grok.com, *.grok.com, *.x.ai, *.x.com, *.twitter.com + Google/Apple OAuth |
| claude | Claude | claude.ai | claude.ai, *.claude.ai, *.anthropic.com + Google/Apple OAuth |
| chatgpt | ChatGPT | chatgpt.com | chatgpt.com, *.openai.com, *.oaiusercontent.com + Google/Apple/MS OAuth |
| gemini | Gemini | gemini.google.com | gemini.google.com, *.google.com, *.gstatic.com, *.googleapis.com |
| perplexity | Perplexity | perplexity.ai | perplexity.ai, *.perplexity.ai + Google/Apple OAuth |
| copilot | Copilot | copilot.microsoft.com | copilot.microsoft.com, m365.cloud.microsoft, *.microsoft.com, *.live.com, *.bing.com |

### 7.2 공통 OAuth 도메인

- Google: accounts.google.com, accounts.youtube.com, myaccount.google.com, ssl.gstatic.com, accounts.gstatic.com, apis.google.com
- Apple: appleid.apple.com, idmsa.apple.com
- Microsoft: login.microsoftonline.com, login.live.com

### 7.3 탭 관리

- 사이드바 클릭: 해당 서비스 탭이 없으면 생성, 있으면 전환
- 서비스당 최대 1탭
- 탭 닫기: WebView2 Detach + Dispose
- 모든 탭 닫기 시 기본 서비스(Grok) 탭 자동 생성
- 탭 드래그 재정렬 (ListBox Drag&Drop)

## 8. 보안/정책
- 도메인 화이트리스트 강제
- HTTPS만 허용
- 외부 URL은 ShellExecute로 브라우저 열기
- WebView2 설정 최소 권한 유지

## 9. MSIX 배포

### 9.1 패키징 자산
- packaging/AppxManifest.xml
- packaging/Assets (아이콘)
- packaging/pack-msix.ps1

### 9.2 빌드 방법
1) 앱 퍼블리시: pack-msix.ps1에서 자동 수행
2) MSIX 생성: makeappx.exe 필요 (Windows SDK 설치)
3) 서명: signtool.exe로 서명 필요

## 10. 오류 처리

### 10.1 전역 예외 핸들러 (App.xaml.cs)
- `DispatcherUnhandledException`: UI 스레드 예외 → MessageBox 표시, 앱 종료 방지
- `AppDomain.UnhandledException`: 도메인 수준 예외 → Debug 로깅
- `TaskScheduler.UnobservedTaskException`: async 미관찰 예외 → 관찰 처리

### 10.2 WebView2 초기화 (TabView.xaml.cs)
- `EnsureCoreWebView2Async` 실패 시 MessageBox로 안내
- WebView2 런타임 미설치 등 환경 문제에 대응

### 10.3 네비게이션 오류 (TabViewModel.cs)
- `NavigationCompleted`에서 `IsSuccess` 확인
- Navigate(), 기타 호출을 try-catch로 보호

### 10.4 탭 리소스 정리
- `TabViewModel.Detach()`: CoreWebView2 이벤트 핸들러 해제
- `TabView.OnUnloaded`: WebView2 Dispose
- 탭 닫기 시 `MainViewModel.CloseTab`에서 호출

## 11. UI 디자인 (WPF-UI)

### 11.1 적용 라이브러리
- WPF-UI v4.2.0 (`http://schemas.lepo.co/wpfui/2022/xaml`)
- Windows 11 Fluent Design System 준수

### 11.2 주요 구성
- `FluentWindow`: Mica/Acrylic 배경 자동 적용, `ExtendsContentIntoTitleBar="True"`
- `ui:TitleBar`: 윈도우 드래그/최소화/최대화/닫기 내장
- `ui:Button`: Fluent 스타일 버튼
- 사이드바: 48px 폭, 서비스 로고 이미지 (24x24 PNG)

### 11.3 테마 전환
- `Wpf.Ui.Appearance.ApplicationThemeManager.Apply()` 사용
- Dark → Light → HighContrast 순환
- 사이드바 하단 아이콘 버튼 (🌙/☀️/🔳)
