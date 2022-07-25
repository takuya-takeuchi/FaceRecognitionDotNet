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
	
run-test-ubuntu16-cpu-x64-desktop: ## run build container for ubuntu16 cpu x64 on desktop
	@pwsh RunTestContainer.ps1 cpu 64 desktop ubuntu 16 "/x64"

run-test-ubuntu16-mkl-x64-desktop: ## run build container for ubuntu16 mkl x64 on desktop
	@pwsh RunTestContainer.ps1 mkl 64 desktop ubuntu 16 "/x64"