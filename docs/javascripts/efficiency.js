

var app = angular.module('EffectivenessApp', ['ui.bootstrap']);

app.service('sharedVars', function () {

	base = { 'refresh' : 0,
			 'modules': [],
			 'mm' : modules
		   };

    return {

        getBase: function() {
        	return base;
        }

    };

})

app.controller('NavBarCtrl', function($scope,sharedVars) {

	$scope.base = sharedVars.getBase();
	$scope.$watch('model.base.refresh', function(newValue) {
		updateBase(base)
	},true);

});

app.controller('InputCtrl', function($scope,sharedVars) {

	$scope.model =  {  
		'base' : sharedVars.getBase(),
		'moduleTypes' : modules
	}

    $scope.change = function() {
    	console.log("change");
        $scope.model.base.refresh ++;
        updateBase($scope.model.base);
    };

    $scope.addModule = function(moduleType) {
    	$scope.model.base.modules.unshift(newModule(moduleType));
        $scope.change();
    };

    $scope.addModule(modules[0].code);

});

function newModule(moduleType) {
	engineersByStars = initCrew('Engineers');
    scientistByStars = initCrew('Scientists');
    pilotsByStars    = initCrew('Pilots');
    touristsByStars  = initCrew('Tourists');

	crew = [engineersByStars, scientistByStars, pilotsByStars, touristsByStars];

	module = {
		'code' : moduleType,
		'crew' : crew,
		'title' : findModule(moduleType).title
	}
	return module;
}

function initCrew(profession) {
	return { 
		'profession' : profession,
		'val' : [0,0,0,0,0,0]
		 };
}

function updateBase(base) {

	base.totalCrew = 0;
	base.happiness = 0;
	
	base.crewCapacity = 0;
	base.workSpace = 0;
	base.livingSpace = 0;

	base.totalCrew = totalCrew(base);

	for (var i = 0; i< base.modules.length; i++) {
		var module = base.modules[i];
		updateModule(module,base.happiness);
		base.workSpace += module.workSpace;
		base.livingSpace += module.livingSpace;
		base.crewCapacity += module.crewCapacity;
	}

	base.happiness = happinessFactor(base.totalCrew,base.livingSpace);
	base.loneliness = lonelinessFactor(base.totalCrew);

	for (var i = 0; i< base.modules.length; i++) {
		var module = base.modules[i];
		updateModule(module,base.happiness);
	}

	return base.totalCrew;
}

function updateModule(module,vesselHappiness) {
	
	module.workSpace = workspaceCount(module.code);
	module.livingSpace = livingspaceCount(module.code);
	module.crewCapacity = capacityCount(module.code);

	//The effectiveness modifier for a given Kerbal is the product of their career factor and the happiness factor for the entire vessel.
	module.effectiveness = moduleCrewEff(module.crew) * vesselHappiness;
	
}

function totalCrew(base) {

	var totalCrew = 0;

	for (var i = 0; i< base.modules.length; i++) {
		var module = base.modules[i];
		totalCrew += sumCrewCount(module.crew);
	}

	return totalCrew;

}

function sumCrewCount(crew) {

	function accumulate(array) {
		acc = 0;
		for (var i = 0; i< array.val.length; i++) {
			acc += array.val[i];
		}

		return acc;
	}

	acc2 = 0;
	for (var i = 0; i< crew.length; i++) {
		acc2 += accumulate(crew[i])
	}

	return acc2;

}


//https://github.com/BobPalmer/MKS/wiki/Efficiency-and-Load

function careerFactor(stars,profession) {
//Each Kerbal has an experience level from 0 to 5 (number of stars next to them) and a profession which carries a multiplier of 1.5 for engineers, 1.0 for scientists, 0.5 for pilots. Tourists provide no value. Career factor is ½ their level times the value of their profession, with a minimum value of 0.05 given to level 0 pilots.
	minFactor = 0.05;

	switch (profession) {
		case 'Engineers':
			trait = 1.5; break;
		case 'Scientists':
			trait = 1.0;  break;
		case 'Pilots':
			trait = 0.5;  break;			
		default:
			return 0; // tourists çan't work
	}

	factor = trait / 2 * stars;

	if ( factor < minFactor ) {
		factor = minFactor;
	}

	return factor;

}

function happinessFactor(totalCrew,totalLivingSpace) {

	var happiness = totalLivingSpace/totalCrew;	

	if ( happiness > 1.5) {
		happiness = 1.5;
	} else if ( happiness < 0.5 ) {
		happiness = 0.5;
	}

	return happiness;
//Kerbals have a happiness factor which is the simple ratio of the amount of living space divided by number of Kerbals on the entire vessel (livingSpace / shipCrew). 
//Living space and workspace are different measurements and separate from capacity. 
//Minimum happiness is 0.5 and maximum is 1.5.
}


// display as k
function moduleCrewEff (crew) {
	// compute effectivenessFactor for all kerbals in this module and sum them all

	var eff = 0;
	for (var i = 0; i< crew.length; i++) {

		prof = crew[i];

		for (var stars = 0 ; stars < prof.val.length ; stars ++ ) {
			eff += careerFactor(stars,prof.profession) * prof.val[stars];
		}
	}

	return eff;
}


// display as m
function activeConverters() {
}

function effectivenessParts() {
// this is a per module value, that searchs the base for effectiveness parts such as MKV workshop for the MKIII fabrication etc
// still no idea on how to compute that, I guess we need a big table here
}

function crewBonus() {
// need clarification on mechanics, is this a base wide value or per module?
}

// boundedEfficiency = 0.25 <= (workSpace/vesselCrew * moduleCrewEff + workSpace/vesselCrew * crewBonus)/converters <= 2.5
// totalEfficiency = boundedEfficiency + effParts

function lonelinessFactor(totalCrew) {
	// TODO get formula
	if ( totalCrew < 5) {
		return 0;
	} else {
		return 1;
	} 
}


function findModule(moduleName) {
	for (var i = 0; i< modules.length; i++) {
		if ( modules[i].code == moduleName ) {
			return modules[i];
		}
	}

	return {};

}


// display as s
function workspaceCount(moduleName) {

	var count = findModule(moduleName).workSpace;
	if ( count === undefined ) {
		count = 0;
	}

	return count;

}

function livingspaceCount(moduleName) {
	var count = findModule(moduleName).livingSpace;
	if ( count === undefined ) {
		count = 0;
	}

	return count;
}

function capacityCount(moduleName) {
	var count = findModule(moduleName).crewCapacity;
	if ( count === undefined ) {
		count = 0;
	}

	return count;
}


//Mark-II modules such as the Pioneer Module, Aeroponics Module, and Kerbitat have workspace values of 1. 
//Mark-III modules such as the Training Akademy and Mobile Refinery have workspace values of 5. 
//Mark-V modules such as the Command Pod and the Colony Hub have workspace values of 1. 
//The MKS Workspace Module has a workspace value of 2. The OKS Inflatable Workspace has a workspace value of 4. 
//The Mark-V Inflatable Workshop has a workspace value of 4.

//The only modules with living space are the OKS Habitation Ring with living space for 10, 
//   and the MK-V Inflatable Habitation Module with living space for 4.