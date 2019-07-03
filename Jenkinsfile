#!groovy

def errorMessage

def getSource()
{
    def git_url = "https://github.com/takuya-takeuchi/FaceRecognitionDotNet"
    def git_branch = "develop"
    def work
    def faceRecognitionDotNet

    if(isUnix())
    {
        work = env.WORKSPACE + "/work"
        faceRecognitionDotNet = work + "/FaceRecognitionDotNet"

        echo 'work: ' + work
        echo 'faceRecognitionDotNet: ' + faceRecognitionDotNet

        if (!fileExists(work))
        {
            sh 'mkdir -p ' + work
        }

        if (!fileExists(faceRecognitionDotNet))
        {
            dir(work)
            {
                sh 'git clone -b ' + git_branch + ' ' + git_url
            }
        }
    }
    else
    {
        work = env.WORKSPACE + "\\work"
        faceRecognitionDotNet = work + "\\FaceRecognitionDotNet"

        echo 'work: ' + work
        echo 'faceRecognitionDotNet: ' + faceRecognitionDotNet

        if (!fileExists(work))
        {
            bat 'mkdir ' + work
        }

        if (!fileExists(faceRecognitionDotNet))
        {
            dir(work)
            {
                bat 'git clone -b ' + git_branch + ' ' + git_url
            }
        }
    }

    return faceRecognitionDotNet
}

def initialize(root)
{
    dir(root)
    {
        if(isUnix())
        {
            sh 'git clean -fxd nuget'
            sh 'git checkout .'
            sh 'git pull origin develop'
            sh './Initialize.sh'
        }
        else
        {
            bat 'git clean -fxd nuget'
            bat 'git checkout .'
            bat 'git pull origin develop'
            bat 'Initialize.bat'
        }
    }
}

def preparation()
{
    def faceRecognitionDotNet = getSource()
    initialize(faceRecognitionDotNet)
}

def getNugetDir(faceRecognitionDotNet)
{
    if(isUnix())
    {
        return faceRecognitionDotNet + "/nuget"
    }
    else
    {
        return faceRecognitionDotNet + "\\nuget"
    }
}

def getDockerDir(faceRecognitionDotNet)
{
    if(isUnix())
    {
        return faceRecognitionDotNet + "/src/DlibDotNet/docker"
    }
    else
    {
        return faceRecognitionDotNet + "\\src\\DlibDotNet\\docker"
    }
}

def getArtifactsDir(faceRecognitionDotNet)
{
    if(isUnix())
    {
        return getNugetDir(faceRecognitionDotNet) + "/artifacts"
    }
    else
    {
        return getNugetDir(faceRecognitionDotNet) + "\\artifacts"
    }
}

def buildContainer()
{
    def faceRecognitionDotNet
    def buildWorkSpace

    stage("Initialize")
    {
        faceRecognitionDotNet = getSource()
        initialize(faceRecognitionDotNet)
    }

    stage('Build DlibDotNet Container')
    {
        buildWorkSpace = getDockerDir(faceRecognitionDotNet)
        dir(buildWorkSpace)
        {
            if(isUnix())
            {     
                sh 'powershell ./build_devel.ps1'  
                sh 'powershell ./build_runtime.ps1'    
            }
            else
            {
                bat 'powershell .\\build_devel.ps1'  
                bat 'powershell .\\build_runtime.ps1'    
            }
        }   
    }
}

def test(script, stashName)
{
    echo 'script: ' + script
    echo 'stashName: ' + stashName

    def faceRecognitionDotNet
    def buildWorkSpace
    def artifactsSpace

    stage("Initialize")
    {
        faceRecognitionDotNet = getSource()
        initialize(faceRecognitionDotNet)
    }

    stage('Test')
    {
        buildWorkSpace = getNugetDir(faceRecognitionDotNet)
        artifactsSpace = getArtifactsDir(faceRecognitionDotNet)

        dir(buildWorkSpace)
        {
            if(isUnix())
            {
                unstash 'nupkg'
                sh 'git checkout .'
                sh script
            }
            else
            {
                unstash 'nupkg'
                bat 'git checkout .'
                bat script
            }
        }
    }

    stage('Results')
    {
        dir(buildWorkSpace)
        {
            stash name: stashName, includes: 'artifacts/test/**/*.trx', excludes: '*.log'
        }
    }
}

node('master')
{
    try
    {
        def props
        stage("Preparation")
        {
            if(isUnix())
            {
                def file = env.JENKINS_HOME + '/FaceRecognitionDotNet.json'
                props = readJSON file: file
            }
            else
            {
                def file = env.JENKINS_HOME + '\\FaceRecognitionDotNet.json'
                props = readJSON file: file
            } 
        }
        
        stage("Build Container")
        {
            def nodeName = props['build']['linux-node']
            node(nodeName)
            {
                buildContainer()
            }
        }

        stage("Packaging")
        {
            def nodeName = props['packaging']['node']
            node(nodeName)
            {
                echo 'Get source code'
                def faceRecognitionDotNet = getSource()
                initialize(faceRecognitionDotNet)

                buildWorkSpace = getNugetDir(faceRecognitionDotNet)
                artifactsSpace = getArtifactsDir(faceRecognitionDotNet)

                echo 'Create packages'
                dir(buildWorkSpace)
                {
                    stage('Build FaceRecognitionDotNet Source')
                    {
                        bat 'BuildNuspec.Pre.bat'
                    }

                    stage('Build Native FaceRecognitionDotNet Source')
                    {
                        parallel 'CPU':
                        {
                            stage('Build FaceRecognitionDotNet.CPU')
                            {
                                bat 'BuildNuspec.CPU.bat'
                            }
                        }, 'CUDA': {
                            stage('Build FaceRecognitionDotNet.CUDA')
                            {
                                bat 'BuildNuspec.CUDA.bat'
                            }
                        }, 'MKL': {
                            stage('Build FaceRecognitionDotNet.MKL')
                            {
                                bat 'BuildNuspec.MKL.bat'
                            }
                        }, 'ARM': {
                            stage('Build FaceRecognitionDotNet.ARM')
                            {
                                bat 'BuildNuspec.ARM.bat'
                            }
                        }
                    }

                    stash name: 'nupkg', includes: '**/*.nupkg'
                }
            }
        }

        stage("Test")
        {
            def builders = [:]

            builders['windows'] =
            {
                def nodeName = props['test']['windows-node']
                node(nodeName)
                {
                    echo 'Test on Windows'
                    test('powershell .\\TestPackageWindows.ps1 ' + params.Version, 'test-windows')
                }
            }
            builders['linux'] =
            {
                def nodeName = props['test']['linux-node']
                node(nodeName)
                {
                    echo 'Test on Linux'
                    test('powershell .\\TestPackageUbuntu16.ps1 ' + params.Version, 'test-linux')
                }
            }
            builders['linux-arm'] =
            {
                def nodeName = props['test']['linux-arm-node']
                node(nodeName)
                {
                    echo 'Test on Linux-ARM'
                    withEnv(["PATH+LOCAL=/usr/local/share/dotnet"])
                    {
                        test('./TestPackageRaspberryPi.sh ' + params.Version, 'test-linux-arm')
                    }
                }
            }
            builders['osx'] =
            {
                def nodeName = props['test']['osx-node']
                node(nodeName)
                {
                    echo 'Test on OSX'
                    withEnv(["PATH+LOCAL=/usr/local/share/dotnet"])
                    {
                        test('./TestPackageOSX.sh ' + params.Version, 'test-osx')
                    }
                }
            }

            parallel builders
        }

        stage("result")
        {
            def nodeName = props['packaging-node']
            node(nodeName)
            {
                dir(buildWorkSpace)
                {
                    unstash 'nupkg'
                    unstash 'test-windows'
                    // unstash 'test-linux'
                    // unstash 'test-linux-arm'
                    unstash 'test-osx'

                    archiveArtifacts artifacts: 'artifacts/test/**/*.*'
                    archiveArtifacts artifacts: '*.nupkg'
                }
            }
        }
    }
    catch (err)
    {
        errorMessage = "${err}"
        currentBuild.result = "FAILURE"

        echo errorMessage
    }
    finally
    {
        if(currentBuild.result != "FAILURE")
        {
            currentBuild.result = "SUCCESS"
        }
    }
}