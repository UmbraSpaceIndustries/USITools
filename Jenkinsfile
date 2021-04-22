pipeline {
  agent { label "windows" }
  stages {
    stage("Build for development") {
      when {
        anyOf {
          branch "experimental"
          branch "main"
        }
      }
      steps {
        bat "dotnet build --output FOR_RELEASE/GameData/000_USITools --configuration debug --verbosity detailed ./USITools/USIToolsUI/USIToolsUI.csproj"
        bat "dotnet build --output FOR_RELEASE/GameData/000_USITools --configuration debug --verbosity detailed ./USITools/USITools/USITools.csproj"
      }
    }
    stage("Build for release") {
      when {
        anyOf {
          branch "prerelease"
          branch "release"
        }
      }
      steps {
        bat "dotnet build --output FOR_RELEASE/GameData/000_USITools --configuration release --verbosity detailed ./USITools/USIToolsUI/USIToolsUI.csproj"
        bat "dotnet build --output FOR_RELEASE/GameData/000_USITools --configuration release --verbosity detailed ./USITools/USITools/USITools.csproj"
      }
    }

    stage("Package for pre-release") {
      steps {
        powershell "Copy-Item ./*.txt ./FOR_RELEASE/GameData/"
        script {
          zip dir: "FOR_RELEASE", zipFile: "USITools.zip", archive: true
        }
      }
    }
  }
}
