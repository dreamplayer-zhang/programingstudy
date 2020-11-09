# Root
신규 검사 프로젝트입니다. 1-PC 사양으로 구현되어 있습니다




※주의
해당 프로젝트에는 외부 라이브러리(OpenCV) dll 파일이 필요합니다. 용량이 큰 관계로 해당 리포지토리에서 제외되어 있습니다. 추후 추가가 가능한 경우 추가할 예정입니다

이하 경로에서 파일을 받은 후 Lib폴더 안에 넣어주어야 정상적으로 실행이 가능합니다
https://ati5344.sharepoint.com/:u:/s/SW1/Ebw10le_SzlPqDMpOOhSw7MBJsuflKh4IcvCAkm0U457-Q?e=Sm8Ej8

SaperaLTSDKSetup8.51.01.2023
https://ati5344-my.sharepoint.com/:u:/g/personal/joseph_ati2000_co_kr/ETMJ0pqp2aVGs_N_BhYxU_QB-PabWtPMdoMzkNjXMtj-Iw?e=2mcDaB

해당 파일의 복사 행동이 빌드 후 이벤트에 등록되어있으므로 그 부분을 참고하시기 바랍니다

## Troubleshooting

* **using SPIIPLUSCOM660Lib 관련 에러가 발생할 시에는 Lib/ACS_Redistribution_Package 폴더를 참조하시기 바랍니다**

* **opencv 관련 에러의 경우 C++ include 설정 혹은 구성관리자가 x64나 Any CPU로 설정되어 있는지 확인 바랍니다**


##각 프로젝트 설명

Root : RootTools Test용

Root_ASIS : ASIS-1000 소팅기

Root_AUP01 : Tape 포장기

Root_AxisMapping : Stage 축 Mapping Test용

Root_EFEM : Wind2용 EFEM

Root_Inspect : 검사용 실행파일 분리

Loot_LogView : RootTools.Logs에서 저장한 Log Viewer

Root_MarsLogViewer : MarsLog 저장 (EFEM, Vision with TCP/IP)

Root_Memory : Memory 저장용 실행파일 분리

Root_TactTime : Hander Tact Time Simulation용

Root_Vega : VEGA
