pipeline {
  agent { label "windows" }
  environment {
    GITHUB_TOKEN = credentials("github-publish-token")
  }
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
          env.IS_PRERELEASE = "true"
        }
      }
    }
    stage("Setup experimental environment") {
      when { branch "experimental" }
      steps {
        script {
          env.BUILD_CONFIG = "debug"
          env.TAG_PREFIX = "Experimental Release"
          env.IS_PRERELEASE = "true"
        }
      }
    }
    stage("Setup pre-release environment") {
      when { branch "prerelease" }
      steps {
        script {
          env.BUILD_CONFIG = "release"
          env.TAG_PREFIX = "Pre-Release"
          env.IS_PRERELEASE = "true"
        }
      }
    }
    stage("Setup release environment") {
      when { branch "release" }
      steps {
        script {
          env.BUILD_CONFIG = "release"
          env.TAG_PREFIX = "Stable Release"
          env.IS_PRERELEASE = "false"
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
    stage("Build") {
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
          env.ARCHIVE_FILENAME = "USITools_${env.GITVERSION_SEMVER}.zip"
          zip dir: "FOR_RELEASE", zipFile: "${env.ARCHIVE_FILENAME}", archive: true
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
    stage("Push release artifacts to GitHub") {
      steps {
        powershell '''
          echo "Creating release on GitHub..."
          $Url = "https://api.github.com/repos/tjdeckard/USITools/releases"
          $Headers = @{
            "Accept" = "application/vnd.github.v3+json"
            "Authorization" = "token $env:GITHUB_TOKEN"
          }
          $Body = @{
            tag_name = "$env:PUBLISH_TAG"
            name = "$env:TAG_PREFIX $env:GITVERSION_SEMVER"
            prerelease = ($env:IS_PRERELEASE -eq "true")
          }
          $Json = ConvertTo-Json $Body
          $Response = Invoke-RestMethod -Method Post -Uri $Url -Headers $Headers -Body $Json

          echo "Uploading artifacts to GitHub..."
          $UploadUrl = $Response | Select -ExpandProperty "upload_url"
          $UploadUrl = $UploadUrl -replace '\{\?name\}', "?name=$env:ARCHIVE_FILENAME"
          $Response = Invoke-RestMethod -Method Post -Uri $UploadUrl -Headers $Headers -ContentType "application/zip" -InFile $env:ARCHIVE_FILENAME
        '''
      }
    }
  }
}
