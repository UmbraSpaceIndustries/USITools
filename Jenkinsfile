pipeline {
  agent { label "windows" }
  stages {
    // Determine the version number
    stage("Calculate semver") {
      steps {
        bat "gitversion /output buildserver"
        script {
          def props = readProperties file: "gitversion.properties"
          env.GITVERSION_SEMVER = props.GitVersion_SemVer
          env.PUBLISH_TAG = "v${props.GitVersion_SemVer}"
        }
      }
    }
    // Build stages for each branch
    stage("Build for bleeding edge") {
      when { branch "main" }
      steps {
        bat "dotnet build --output FOR_RELEASE/GameData/000_USITools --configuration debug --verbosity detailed ./USITools/USIToolsUI/USIToolsUI.csproj"
        bat "dotnet build --output FOR_RELEASE/GameData/000_USITools --configuration debug --verbosity detailed ./USITools/USITools/USITools.csproj"
      }
    }
    stage("Build for experimental") {
      when { branch "experimental" }
      steps {
        bat "dotnet build --output FOR_RELEASE/GameData/000_USITools --configuration debug --verbosity detailed ./USITools/USIToolsUI/USIToolsUI.csproj"
        bat "dotnet build --output FOR_RELEASE/GameData/000_USITools --configuration debug --verbosity detailed ./USITools/USITools/USITools.csproj"
      }
    }
    stage("Build for pre-release") {
      when { branch "prerelease" }
      steps {
        bat "dotnet build --output FOR_RELEASE/GameData/000_USITools --configuration release --verbosity detailed ./USITools/USIToolsUI/USIToolsUI.csproj"
        bat "dotnet build --output FOR_RELEASE/GameData/000_USITools --configuration release --verbosity detailed ./USITools/USITools/USITools.csproj"
      }
    }
    stage("Build for release") {
      when { branch "release" }
      steps {
        bat "dotnet build --output FOR_RELEASE/GameData/000_USITools --configuration release --verbosity detailed ./USITools/USIToolsUI/USIToolsUI.csproj"
        bat "dotnet build --output FOR_RELEASE/GameData/000_USITools --configuration release --verbosity detailed ./USITools/USITools/USITools.csproj"
      }
    }
    // Packaging
    stage("Package for release") {
      steps {
        powershell "Copy-Item ./*.txt ./FOR_RELEASE/GameData/"
        script {
          zip dir: "FOR_RELEASE", zipFile: "USITools_${env.GITVERSION_SEMVER}.zip", archive: true
        }
      }
    }
    // Tag commit, if necessary
    stage("Tag commit for bleeding edge") {
      when { branch "main" }
      steps {
        powershell '''
          echo "looking for tag $env:PUBLISH_TAG"
          $tagFound = git tag -l "$env:PUBLISH_TAG"
          if ( $tagFound -ne $env:PUBLISH_TAG )
          {
            echo "found tag"
            git tag -a $env:PUBLISH_TAG -m "Unstable Release $env:GITVERSION_SEMVER"
            git push --tags
          }
        '''
      }
    }
    // Push artifacts to GitHub
    // stage("Push release artifacts to GitHub") {
    //   // TODO - Figure out pushing to GitHub (probably going to have to automate creating new tags first tho)
    //   steps {}
    // }
  }
}
