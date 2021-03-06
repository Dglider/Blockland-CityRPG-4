// Election
function CityMayor_startElection()
{
	messageAll('',"\c6 - \c2Election has begun!");
	CityMayor_resetCandidates();
	$City::Mayor::Mayor::ElectionID = getRandom(1, 30000);
	$Pref::Server::City::Mayor::Active = 0;
	$City::Mayor::Voting = 1;
	$City::Mayor::ID = -1;
	$City::Mayor::String = "\c2Election has begun!";
	messageClient(%client, '', "\c6" @ $City::Mayor::Mayor::ElectionID);
	%time = $Pref::Server::City::Mayor::Time * 60000;
	$City::Mayor::Schedule = schedule(%time, 0, CityMayor_stopElection);
}

function serverCmdstopElection(%client)
{
	%client.cityLog("/stopElection");
	if(%client.isAdmin)
	{
		CityMayor_stopElection();
	}
}

function CityMayor_stopElection()
{
	%winner = CityMayor_getWinner();

	$City::Mayor::String = getField(%winner, 0);
	$City::Mayor::ID = getField(%winner, 1);

	$Pref::Server::City::Mayor::Active = 1;
	$City::Mayor::Voting = 0;
	CityMayor_resetCandidates();
	CityMayor_resetimpeachers();
	messageAll('', "\c3The election has ended!");

	if($City::Mayor::String $= "")
	{
		messageAll('', "\c3Nobody won the election. The spot for mayor will remain empty.");
		$City::Mayor::String = "None";
	}
	else
	{
		messageAll('', "\c3" @ $City::Mayor::String SPC "\c6has won the election!");

		%client = findClientByBL_ID($City::Mayor::ID);
		messageClient(%client, '', "\c6Congratulations, you are now the" SPC JobSO.job[$City::MayorJobID].name @ "\c6! Your new salary is \c3$" @ JobSO.job[$City::MayorJobID].pay @ "\c6 per day.");
		jobset(%client, $City::MayorJobID);

	}
}

//restart
function serverCmdrestartElection(%client)
{
	%client.cityLog("/restartElection");

	if(%client.isAdmin)
	{
		CityMayor_resetCandidates();
		$City::Mayor::Mayor::ElectionID = getRandom(1, 30000);
		$Pref::Server::City::Mayor::Active = 1;
		$City::Mayor::String = "\c2Election\c6 has begun!";
		messageClient(%client, '', "\c6" @ $City::Mayor::Mayor::ElectionID);
	}
}

//vote
function serverCmdvoteElection(%client, %arg2)
{
	%client.cityLog("/voteElection");

	if(isObject(%arg1 = findClientByName(%arg2)))
	{
		if(%arg1 $= "") //if not blank
		{
			messageClient(%client, '', "Please try putting in a user's name.");
		} else {
			if($City::Mayor::Voting == 1 && $Pref::Server::City::Mayor::Active == 0) //if election
			{
				if(CityRPGData.getData(%client.bl_id).valueElectionID != $City::Mayor::Mayor::ElectionID) //if hasn't voted
				{
					if(CityMayor_getCandidatesTF(%arg1.name))
					{
						messageClient(%client, '', "\c6You have voted for\c3" SPC %arg1.name @ "\c6.");
						CityRPGData.getData(%client.bl_id).valueElectionID = $City::Mayor::Mayor::ElectionID;
						%voteIncrease = getMayor($City::Mayor::Mayor::ElectionID, %arg1.name) + 1;
						inputMayor($City::Mayor::Mayor::ElectionID, %arg1.name, %voteIncrease);
					} else {
						messageClient(%client, '', "This player isn't a candidate.");
					}
				} else {
					messageClient(%client, '', "You've already voted!");
				}
			} else {
				messageClient(%client, '', "There isn't an election.");
			}
		}
	}
}

//register
function serverCmdRegisterCandidates(%client)
{
	%client.cityLog("/registerCandidates");

	if(CityRPGData.getData(%client.bl_id).valueMoney >= $Pref::Server::City::Mayor::Cost)
	{
		CityMayor_inputCandidates(%client.name, %client.bl_id);
		messageClient(%client, '', "\c6Congratulations, you are now a candidate for the election.");
		CityRPGData.getData(%client.bl_id).valueMoney -= $Pref::Server::City::Mayor::Cost;
		%client.setInfo();
	} else {
		messageClient(%client, '', "\c6You don't have $" @ $Pref::Server::City::Mayor::Cost @ "!");
	}
}

//setJob
function serverCmdMeMayor(%client)
{
	%client.cityLog("/meMayor");

	if(%client.name $= $City::Mayor::String)
	{
		jobset(%client, $City::MayorJobID);
	}
}

// Looper
function CityMayor_refresh()
{
	$City::Mayor::Mayor::Requirement = 10;
	if($Pref::Server::City::Mayor::Active == 0) //if active mayor
	{
		if((clientGroup.getCount() >= $City::Mayor::Mayor::Requirement) || ($City::Mayor::Force::Start == 1))
		{
			if($City::Mayor::Voting == 0)
				CityMayor_startElection();
		} else if($City::Mayor::Voting == 1) {

		} else {
			$City::Mayor::ID = -1;
			$City::Mayor::Voting = 0;
			$City::Mayor::String = "Required Players: \c3" SPC clientGroup.getCount() @ "\c6/" @ $City::Mayor::Mayor::Requirement;
		}
	} else if($City::Mayor::Voting == 0 && $Pref::Server::City::Mayor::Active == 0) {
			$City::Mayor::ID = -1;
			$City::Mayor::String = "Required Players: \c3" SPC clientGroup.getCount() @ "\c6/" @ $City::Mayor::Mayor::Requirement;
	}
}

//databases
function CityMayor_inputCandidates(%string, %id)
{
	for(%i = 0; %i < 25; %i++)
	{
		if($candidates[%i] $= "") {
			$candidates[%i] = %string;
			$candidateIDs[%i] = %id;
			%i = 26;
		} else if($candidates[%i] $= %string) {
			%i = 26;
		}
	}
}

function CityMayor_getCandidates(%client)
{
	messageClient(%client,'',"\c6List of candidates:");
	%listnum = 0;
	for(%i = 0; %i < 25; %i++)
	{
		if($candidates[%i] $= "")
		{
		} else {
			%listnum++;
			messageClient(%client,'',"-\c6" @ %listnum @ "\c0-\c6" @ $candidates[%i]);
		}
	}
}

function CityMayor_getCandidatesTF(%arg1)
{
	for(%i = 0; %i < 25; %i++)
	{
		if($candidates[%i] $= %arg1)
		{
			return true;
		}
	}
	return false;
}

function CityMayor_resetCandidates(%client)
{
	messageClient(%client,'',"All candidates have been reset");
	for(%i = 0; %i < 25; %i++)
	{
		$candidates[%i] = "";
	}
}

//other
function serverCmdmayorForceStart(%client)
{
	%client.cityLog("/mayorForceStart");

	if(%client.isAdmin)
	{
		if($City::Mayor::Force::Start == 0)
		{
			$City::Mayor::Force::Start = 1;
			$Pref::Server::City::Mayor::Active = 0;
			messageClient(%client, '', "\c2Enabled");
		} else {
			$City::Mayor::Force::Start = 0;
			messageClient(%client, '', "Disabled");
		}
	}
}

function serverCmdtopC(%client)
{
	%client.cityLog("/topC");
	messageClient(%client,'',"\c6Candidates:");
	%listnum = 0;
	for(%i = 0; %i < 25; %i++)
	{
		if($candidates[%i] !$= "")
		{
			%listnum++;
			%votes = getMayor($City::Mayor::Mayor::ElectionID, $candidates[%i]);
			if(%votes $= "")
				%votes = 0;

			messageClient(%client,'',"-\c6" @ %listnum @ "\c0-\c6" @ $candidates[%i] SPC "\c6has\c0" SPC %votes SPC "\c6votes!");
		}
	}
}

function CityMayor_getWinner()
{
	%top = 0;
	%toBeat = "";
	for(%i = 0; %i < 25; %i++)
	{
		if($candidates[%i] !$= "")
		{
			%current = getMayor($City::Mayor::Mayor::ElectionID, $candidates[%i]);
			if(%current > %top)
			{
				%toBeat = $candidates[%i];
				%toBeatID = $candidateIDs[%i];
				%top = %current;
			}
		}
	}
	return %toBeat TAB %toBeatID;
}
