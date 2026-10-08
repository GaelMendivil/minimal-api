pipeline {
    agent { label 'build-agent' }

    options {
        skipDefaultCheckout(true)
        timestamps()
        disableConcurrentBuilds()
    }

    environment {
        CONFIGURATION = 'Release'
        REGISTRY      = 'localhost:5000'
        IMAGE_NAME    = 'minimal-api'
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
                sh 'git log -1 --oneline'
            }
        }

        stage('Build') {
            steps {
                sh '''
                    dotnet restore
                    dotnet build --configuration "$CONFIGURATION" --no-restore
                '''
            }
        }

        stage('Unit Test') {
            steps {
                sh '''
                    rm -rf TestResults
                    dotnet test \
                        --configuration "$CONFIGURATION" \
                        --no-build \
                        --logger "trx;LogFileName=unit-tests.trx" \
                        --results-directory TestResults
                '''
            }
            post {
                always {
                    archiveArtifacts artifacts: 'TestResults/**/*', allowEmptyArchive: true
                }
            }
        }

        stage('Version') {
            steps {
                script {
                    env.SHORT_COMMIT = sh(script: 'git rev-parse --short=7 HEAD', returnStdout: true).trim()
                    env.IMAGE_TAG    = "git-${env.SHORT_COMMIT}"
                    env.IMAGE        = "${env.REGISTRY}/${env.IMAGE_NAME}:${env.IMAGE_TAG}"
                }
                echo "Release image: ${env.IMAGE}"
            }
        }

        stage('Package Image') {
            steps {
                sh '''
                    rm -rf out
                    mkdir -p out

                    dotnet publish src/Api \
                        --configuration "$CONFIGURATION" \
                        --no-build \
                        -p:PublishProfile=DefaultContainer \
                        -p:ContainerRepository="$IMAGE_NAME" \
                        -p:ContainerImageTag="$IMAGE_TAG" \
                        -p:ContainerArchiveOutputPath="$WORKSPACE/out/minimal-api.tar"

                    podman load --input out/minimal-api.tar
                    podman tag "$IMAGE_NAME:$IMAGE_TAG" "$IMAGE"
                '''
            }
        }

        stage('Publish Image') {
            steps {
                sh '''
                    podman push --tls-verify=false "$IMAGE"
                    podman image inspect "$IMAGE"
                '''
            }
        }
    }
}
