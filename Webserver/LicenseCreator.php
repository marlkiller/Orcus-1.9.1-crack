<?php

require_once 'Crypt/RSA.php'; // PHPSec's RSA-Klasse einbinden

// Generierung von Lizenzen in einer separaten Klasse
class LicenseCreator
{
	// Niemals anderen Leuten zugänglich machen!
	const privateKey = '<RSAKeyValue><Modulus>ryKX3yA8fy8w+AagMKv3cedKYzc0O3dIaEF79JNYdBSCY0lYvwJzIchtAv8T8+aVMbSxtMeDADP7USqfgB2Jto2SSkn+QX4lBGy2rVpF9SSmbnAtjZWpJYhKFwbN5PlyqgI5GAnB4g1LEm0GA/CdjophJqTdmQyKUb9Dh2jzzN8=</Modulus><Exponent>AQAB</Exponent><P>vwMstnoAJeUcgcP05lY744/abKF9PZjtpT0rJp9TEHC8nfEobr8Y4WaruvIVhXY1GgZAnQAd0G+GfmGKXiKtaQ==</P><Q>6riJqt8vqXYttOqfQ60rclHwmg7sPKVDqiOS9H1aK9ZyXWh5H4gg36CPmb555wEeYYPCWwcKAK2PSAxV21O3Bw==</Q><DP>rbYkZrsjAVOQjk74nLWV94ku2pYuwOMgVKMBaDmDIDN2xai43aa66NonmXdprRtohYdkIaQmeRXD2ZG5dYzR6Q==</DP><DQ>TIpOwjyzcyRJdVSJCO2gXFAiEGrLWF9f+ExPcJCr5d2xP4qA7OpcJfBaw3zcjZrMyGnJ6BschOoT7h+vo6zh4w==</DQ><InverseQ>m7yVisWuwdGs7A1FloDxPYRDnKjNFHe1YErnD6H4Vyd3JTgcFxZVPkSLe9D1ib+uE+q/N9B/SJsoNCm6fXIrBg==</InverseQ><D>HnG8hdiEO7W35P293gge0SmcOEgR595x81Gi2x68Cx2/lsPazgV+fxpovNMbpFPqjoPzOlJOVvwyTWdD59D6rbvHgtm3zC2euqSG3t4RS7ZsX13Il7y+aGr2QXNjUKWtR8snkGChr1zmlB8SR9lrDewkAPscIf3rHLhCZ7gtLDk=</D></RSAKeyValue>';

	public static function CreateLicense($licensee)
	{
		// Gleiche Generalisierung wie am Client:
		$licenseeGen = self::GeneralizeDataString($licensee);

		$rsa = new Crypt_RSA(); // Neue RSA-Klasse erstellen

		// Setzen der RSA-Optionen auf die, die auch am Client verwendet werden:
		$rsa->setPrivateKeyFormat(CRYPT_RSA_PRIVATE_FORMAT_XML);
		$rsa->setHash('SHA1');
		$rsa->setSignatureMode(CRYPT_RSA_SIGNATURE_PKCS1);

		// privaten Schlüssel laden
		$rsa->loadKey(self::privateKey);

		// Erstellen der Signatur
		$signature = $rsa->sign($licenseeGen);

		// Formatierte Lizenzdaten zurückgeben
		return self::FormatLicense($licensee, $type, $signature);
	}

	private static function FormatLicense($licensee, $type, $signature)
	{
		// Binärdaten aus $signature in hexadezimal kodierten String umwandeln
		$formattedSignature = self::EncodeDataToHexString($signature);

		// Signatur in 29-Zeichen-Blöcke aufteilen (sieht schöner aus)
		$formattedSignature = chunk_split($formattedSignature, 29);

		$l = "--------BEGIN LICENSE--------\n"; // Unser Anfangsblock
		$l .= $licensee . "\n"; // Die hardware id
		$l .= trim($formattedSignature) . "\n"; // die in mehrere Zeilen aufgeteilte, kodierte Signatur
		$l .= "---------END LICENSE---------"; // Ende der Lizenz

		return $l;
	}

	private static function EncodeDataToHexString($data)
	{
		return strtoupper(bin2hex($data));
	}

	private static function GeneralizeDataString($someString)
	{
		// Gleiche Funktion wie am Client
		return strtoupper(self::StripWhiteSpace($someString));
	}

	private static function StripWhiteSpace($someString)
	{
		// Gleiche Funktion wie am Client, nur mit RegEx
		return preg_replace('/\s+/', '', $someString);
	}
}
