<?php
 
require( 'RegAPI.php' );

if ($_SERVER['REQUEST_METHOD'] == 'POST')
{
	$first_name = $_POST['first_name'];
	$last_name = $_POST['last_name'];
	$password = $_POST['password'];
	
	$response = regapiGetCapabilities($first_name, $last_name, $password);
	
	$str = $response->saveXML();
	echo $str;
}
?>
<form action="<?php print $_SERVER['PHP_SELF']; ?>" method="post">
<table>
<tr><td>First Name</td><td><input name='first_name'></td></tr>

<tr><td>Last Name</td><td><input name='last_name'></td></tr>

<tr><td>Password</td><td><input name='password' type='password'></td></tr>

<tr><td> </td></tr>
<tr><td align='center' colspan=2><input type='submit' value="Get Capabilities"></td></tr>
</table>
</form>