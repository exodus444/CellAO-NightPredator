CREATE TABLE  `charactersinventory` (
	`ID` int(32) NOT NULL AUTO_INCREMENT,
	`CharacterID` int(32) NOT NULL,
	`Placement` int(32) NOT NULL DEFAULT '0',
	`Flags` int(32) NOT NULL DEFAULT '0',
	`MultipleCount` int(32) NOT NULL DEFAULT '0',
	`Type` int(32) NOT NULL DEFAULT '0',
	`Instance` int(32) NOT NULL DEFAULT '0',
	`LowID` int(32) NOT NULL DEFAULT '0',
	`HighID` int(32) NOT NULL DEFAULT '0',
	`Quality` int(32) NOT NULL DEFAULT '0',
	`Nothing` int(32) NOT NULL DEFAULT '0',
	`Container` int(11) NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;