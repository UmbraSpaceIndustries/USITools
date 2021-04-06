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
	  	bat "move artifacts/*.dll FOR_RELEASE/GameData"
	  	dir "FOR_RELEASE/GameData"
	  	script {
	  	  zip zipFile: "../USITools.zip", archive: true
	  	}
	  }
    }
  }
}
