.DEFAULT_GOAL := help

help: ## print this message
	@echo "Example operations by makefile."
	@echo ""
	@echo "Usage: make SUB_COMMAND argument_name=argument_value"
	@echo ""
	@echo "Command list:"
	@echo ""
	@printf "\033[36m%-40s\033[0m %-50s %s\n" "[Sub command]" "[Description]" "[Example]"
	@grep -E '^[/a-zA-Z0-9_-]+:.*?## .*$$' $(MAKEFILE_LIST) | perl -pe 's%^([/a-zA-Z0-9_-]+):.*?(##)%$$1 $$2%' | awk -F " *?## *?" '{printf "\033[36m%-40s\033[0m %-50s %s\n", $$1, $$2, $$3}'

run-build-centos7-cpu-x64-desktop: ## run build container for centos7 cpu x64 on desktop
	@pwsh RunBuildContainer.ps1 cpu 64 desktop centos 7 "/x64"

run-build-centos7-mkl-x64-desktop: ## run build container for centos7 mkl x64 on desktop
	@pwsh RunBuildContainer.ps1 mkl 64 desktop centos 7 "/x64"
	
run-build-ubuntu16-cpu-x64-desktop: ## run build container for ubuntu16 cpu x64 on desktop
	@pwsh RunBuildContainer.ps1 cpu 64 desktop ubuntu 16 "/x64"

run-build-ubuntu16-mkl-x64-desktop: ## run build container for ubuntu16 mkl x64 on desktop
	@pwsh RunBuildContainer.ps1 mkl 64 desktop ubuntu 16 "/x64"

run-test-centos7-cpu-x64-desktop: ## run build container for centos7 cpu x64 on desktop
	@pwsh RunTestContainer.ps1 cpu 64 desktop centos 7 "/x64"

run-test-centos7-mkl-x64-desktop: ## run build container for centos7 mkl x64 on desktop
	@pwsh RunTestContainer.ps1 mkl 64 desktop centos 7 "/x64"

run-test-centos7-cu92-x64-desktop: ## run build container for centos7 cuda-92 x64 on desktop
	@pwsh RunTestContainer.ps1 cuda 64 desktop centos 7 "92"

run-test-centos7-cu100-x64-desktop: ## run build container for centos7 cuda-100 x64 on desktop
	@pwsh RunTestContainer.ps1 cuda 64 desktop centos 7 "100"

run-test-centos7-cu101-x64-desktop: ## run build container for centos7 cuda-101 x64 on desktop
	@pwsh RunTestContainer.ps1 cuda 64 desktop centos 7 "101"

run-test-centos7-cu102-x64-desktop: ## run build container for centos7 cuda-102 x64 on desktop
	@pwsh RunTestContainer.ps1 cuda 64 desktop centos 7 "102"

run-test-centos7-cu110-x64-desktop: ## run build container for centos7 cuda-110 x64 on desktop
	@pwsh RunTestContainer.ps1 cuda 64 desktop centos 7 "110"

run-test-centos7-cu111-x64-desktop: ## run build container for centos7 cuda-111 x64 on desktop
	@pwsh RunTestContainer.ps1 cuda 64 desktop centos 7 "111"

run-test-centos7-cu112-x64-desktop: ## run build container for centos7 cuda-112 x64 on desktop
	@pwsh RunTestContainer.ps1 cuda 64 desktop centos 7 "112"
	
run-test-centos7-cpu-x64-desktop: ## run build container for centos7 cpu x64 on desktop
	@pwsh RunTestContainer.ps1 cpu 64 desktop ubuntu 16 "/x64"

run-test-ubuntu16-mkl-x64-desktop: ## run build container for ubuntu16 mkl x64 on desktop
	@pwsh RunTestContainer.ps1 mkl 64 desktop ubuntu 16 "/x64"

run-test-ubuntu16-cu92-x64-desktop: ## run build container for ubuntu16 cuda-92 x64 on desktop
	@pwsh RunTestContainer.ps1 cuda 64 desktop ubuntu 16 "92"

run-test-ubuntu16-cu100-x64-desktop: ## run build container for ubuntu16 cuda-100 x64 on desktop
	@pwsh RunTestContainer.ps1 cuda 64 desktop ubuntu 16 "100"

run-test-ubuntu16-cu101-x64-desktop: ## run build container for ubuntu16 cuda-101 x64 on desktop
	@pwsh RunTestContainer.ps1 cuda 64 desktop ubuntu 16 "101"

run-test-ubuntu16-cu102-x64-desktop: ## run build container for ubuntu16 cuda-102 x64 on desktop
	@pwsh RunTestContainer.ps1 cuda 64 desktop ubuntu 16 "102"

run-test-ubuntu16-cu110-x64-desktop: ## run build container for ubuntu16 cuda-110 x64 on desktop
	@pwsh RunTestContainer.ps1 cuda 64 desktop ubuntu 16 "110"

run-test-ubuntu16-cu111-x64-desktop: ## run build container for ubuntu16 cuda-111 x64 on desktop
	@pwsh RunTestContainer.ps1 cuda 64 desktop ubuntu 16 "111"

run-test-ubuntu16-cu112-x64-desktop: ## run build container for ubuntu16 cuda-112 x64 on desktop
	@pwsh RunTestContainer.ps1 cuda 64 desktop ubuntu 16 "112"