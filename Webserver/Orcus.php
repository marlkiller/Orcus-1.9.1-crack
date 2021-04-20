<?php
require_once 'LicenseCreator.php';

header('Content-Type: text/plain; charset=utf-8');

if (!isset($_POST['method']))
    die("No method was posted");

$servername = "";
$username   = "";
$password   = "";
$dbname     = "";

switch ($_POST['method']) {
    case "r": //register
        if (!isset($_POST["hwid"]))
            die("No hardware id was posted");
        
        if (!isset($_POST["lkey"]))
            die("No license key was posted");
        
        $hardwareId = $_POST["hwid"];
        $licenseKey = $_POST["lkey"];
        
        $conn = new mysqli($servername, $username, $password, $dbname);
        
        $stmt = $conn->prepare('SELECT * FROM licenses WHERE licenseKey = ? LIMIT 1');
        $stmt->bind_param('s', $licenseKey);
        $stmt->execute();
        $result = $stmt->get_result();
        $stmt->close();
        $row;
        while ($tempRow = $result->fetch_assoc()) {
            $row = $tempRow;
        }
        
        if ($row == null) {
            die("1|");
        }
        
        if ($row["banned"] == "1")
            die("2|");
        
        $licenseId = $row["id"];
        
        $result = $conn->query('SELECT * FROM computers WHERE licenseId = ' . $licenseId . ' LIMIT 1');
        
        $freeTicket = false;
        if ($result->num_rows > 0) {
            while ($row1 = $result->fetch_assoc()) {
                if ($row1["hardwareId"] == $hardwareId) {
                    $freeTicket = true;
                    break;
                }
            }
        }
        
        if ($freeTicket == true || $result->num_rows < 10) {
            if ($freeTicket == false) {
                $stmt = $conn->prepare('INSERT INTO computers (hardwareId, licenseId) VALUES (? , ' . $licenseId . ')');
                $stmt->bind_param('s', $hardwareId);
                $stmt->execute();
            } else {
                $stmt = $conn->prepare('UPDATE computers SET Timestamp=now() WHERE licenseId = ' . $licenseId . ' AND hardwareId = ?');
                $stmt->bind_param('s', $hardwareId);
                $stmt->execute();
            }
            
			$stmt = $conn->prepare('INSERT INTO registrations (licenseId, hardwareId) VALUES(' . $licenseId . ', ?)');
            $stmt->bind_param('s', $hardwareId);
            $stmt->execute();
				
            echo "0|" . LicenseCreator::CreateLicense($hardwareId);
        } else {
            die("3|");
        }
        $conn->close();
        break;
    case "u": //update
        
        break;
}
?>