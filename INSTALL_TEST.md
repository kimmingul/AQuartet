# MSIX 설치/업데이트 테스트 가이드

이 문서는 `migration` 기준 MSIX 설치/업데이트 테스트 절차를 제공합니다.

## 1. 준비 사항
- Windows 10/11 (개발자 모드 권장)
- Windows SDK 설치 (makeappx.exe, signtool.exe)
- 서명된 MSIX (또는 테스트용 self-signed 인증서)

## 2. 패키지 생성
```powershell
# MSIX 생성
powershell -NoProfile -ExecutionPolicy Bypass -File migration\packaging\pack-msix.ps1
```

생성물:
- `migration\artifacts\GrokDesktop.msix`

## 3. 인증서/서명 확인
- 서명 완료 후:
```powershell
& "C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe" verify /pa /v migration\artifacts\GrokDesktop.msix
```

## 4. 설치 테스트
### 4.1 PowerShell로 설치
```powershell
Add-AppxPackage -Path migration\artifacts\GrokDesktop.msix
```

### 4.2 설치 확인
- 시작 메뉴에서 “Grok Desktop” 검색
- 앱 실행 후 grok.com 로드 확인
- 탭 생성/닫기 테스트

## 5. 업데이트 테스트
### 5.1 버전 상승
- `migration\packaging\AppxManifest.xml`의 `Identity Version` 값 증가
  - 예: `1.0.0.0` -> `1.0.1.0`

### 5.2 재패키징
```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File migration\packaging\pack-msix.ps1
```

### 5.3 업데이트 설치
```powershell
Add-AppxPackage -Path migration\artifacts\GrokDesktop.msix
```

## 6. 제거 테스트
```powershell
Get-AppxPackage *GrokDesktop* | Remove-AppxPackage
```

## 7. 체크리스트
- [ ] 설치 성공
- [ ] 앱 실행 성공
- [ ] grok.com 접속
- [ ] 로그인 세션 유지
- [ ] 탭 생성/닫기 정상 동작
- [ ] 업데이트 설치 성공
- [ ] 제거 정상 동작

## 8. 문제 해결
- 서명 오류: 인증서/타임스탬프 설정 확인
- 설치 오류: 개발자 모드/정책 확인
- 런타임 오류: WebView2 런타임 설치 확인
