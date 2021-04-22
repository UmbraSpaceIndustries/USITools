pipeline {
  agent { label "windows" }
  stages {
    stage("Calculate semver") {
      steps {
        bat "gitversion /output buildserver"
        script {
          def props = readProperties file: "gitversion.properties"
          env.GITVERSION_SEMVER = props.GitVersion_SemVer
        }
      }
    }
    stage("Build for bleeding edge") {
      when {
        branch "main"
      }
      steps {
        bat "dotnet build --output FOR_RELEASE/GameData/000_USITools --configuration debug --verbosity detailed ./USITools/USIToolsUI/USIToolsUI.csproj"
        bat "dotnet build --output FOR_RELEASE/GameData/000_USITools --configuration debug --verbosity detailed ./USITools/USITools/USITools.csproj"
      }
    }
    stage("Build for experimental") {
      when {
        branch "experimental"
      }
      steps {
        bat "dotnet build --output FOR_RELEASE/GameData/000_USITools --configuration debug --verbosity detailed ./USITools/USIToolsUI/USIToolsUI.csproj"
        bat "dotnet build --output FOR_RELEASE/GameData/000_USITools --configuration debug --verbosity detailed ./USITools/USITools/USITools.csproj"
      }
    }
    stage("Build for pre-release") {
      when {
        branch "prerelease"
      }
      steps {
        bat "dotnet build --output FOR_RELEASE/GameData/000_USITools --configuration release --verbosity detailed ./USITools/USIToolsUI/USIToolsUI.csproj"
        bat "dotnet build --output FOR_RELEASE/GameData/000_USITools --configuration release --verbosity detailed ./USITools/USITools/USITools.csproj"
      }
    }
    stage("Build for release") {
      when {
        branch "release"
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
          zip dir: "FOR_RELEASE", zipFile: "USITools_${env.GITVERSION_SEMVER}.zip", archive: true
        }
      }
    }
  }
}
