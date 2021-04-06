pipeline {
  agent { label "windows" }
  stages {
    stage("build") {
      steps {
        bat "dotnet build --output artifacts --configuration release --verbosity detailed ./USITools/USIToolsUI/USIToolsUI.csproj"
        bat "dotnet build --output artifacts --configuration release --verbosity detailed ./USITools/USITools/USITools.csproj"
        stash includes: "artifacts/*.dll,FOR_RELEASE/**", name: "artifacts"
      }
    }

    stage("package") {
	  steps {
	  	unstash name: "artifacts"
	  	powershell "Move-Item -Force ./artifacts/*.dll ./FOR_RELEASE/GameData/"
	  	script {
	  	  zip dir: "FOR_RELEASE/GameData", zipFile: "USITools.zip", archive: true
	  	}
	  }
    }
  }
}
