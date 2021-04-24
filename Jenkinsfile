pipeline {
  agent { label "windows" }
  stages {
    // Configure git
    stage("Configure git") {
      steps {
        bat "git config user.email burt-macklin@jenkins"
        bat 'git config user.name "Agent Burt Macklin"'
      }
    }
    // Determine build & publish flags for branch
    stage("Setup bleeding edge environment") {
      when { branch "main" }
      steps {
        script {
          env.BUILD_CONFIG = "debug"
          env.TAG_PREFIX = "Unstable Release"
        }
      }
    }
    stage("Setup experimental environment") {
      when { branch "experimental" }
      steps {
        script {
          env.BUILD_CONFIG = "debug"
          env.TAG_PREFIX = "Experimental Release"
        }
      }
    }
    stage("Setup pre-release environment") {
      when { branch "prerelease" }
      steps {
        script {
          env.BUILD_CONFIG = "release"
          env.TAG_PREFIX = "Pre-Release"
        }
      }
    }
    stage("Setup release environment") {
      when { branch "main" }
      steps {
        script {
          env.BUILD_CONFIG = "release"
          env.TAG_PREFIX = "Stable Release"
        }
      }
    }
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
    // Build
    stage("Build for bleeding edge") {
      when { branch "main" }
      steps {
        bat "dotnet build --output FOR_RELEASE/GameData/000_USITools --configuration ${env.BUILD_CONFIG} --verbosity detailed ./USITools/USIToolsUI/USIToolsUI.csproj"
        bat "dotnet build --output FOR_RELEASE/GameData/000_USITools --configuration ${env.BUILD_CONFIG} --verbosity detailed ./USITools/USITools/USITools.csproj"
      }
    }
    // Packaging
    stage("Package artifacts") {
      steps {
        powershell "Copy-Item ./*.txt ./FOR_RELEASE/GameData/"
        script {
          zip dir: "FOR_RELEASE", zipFile: "USITools_${env.GITVERSION_SEMVER}.zip", archive: true
        }
      }
    }
    // Tag commit, if necessary
    stage("Tag commit") {
      steps {
        powershell '''
          echo "Looking for tag $env:PUBLISH_TAG..."
          $tagFound = git tag -l "$env:PUBLISH_TAG"
          if ( $tagFound -ne $env:PUBLISH_TAG )
          {
            echo "Tag not found. Creating tag..."
            git tag -a $env:PUBLISH_TAG -m "$env:TAG_PREFIX $env:GITVERSION_SEMVER"
            echo "Pushing tag to GitHub..."
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
