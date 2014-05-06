<?php
 
////////////////////////////////////////////////////////////////////////////////
//
// Hacked up by Liandra Ceawlin, Version 1, Tue Jul 21 11:53:34 EDT 2009
// Hacked more by Fly-Man- (OpenSim) Version 2, Fri Apr 25 02:31:10 CET 2014
//
//	I'm not really much of a PHP coder, so if you find any bugs or see things
// in this code that are utterly atrocious, please IM me in-world or email
// support@ceawlin.com.  Good luck with it!
//
////////////////////////////////////////////////////////////////////////////////
//
// Yadda yadda so I don't get sued when it breaks.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
//
////////////////////////////////////////////////////////////////////////////////

require( 'config.php' );
 
// Check if the user with firstname, lastname and password can request capabilities
// to create users, get errorcodes or add users to a group.
//
// This function will return Caps message that are used for all the other RegAPI functions
function regapiGetCapabilities ( $firstname, $lastname, $password )
{
	$str = "<llsd><map>";
	$str = $str."<key>first_name</key><string>".$firstname."</string>";
	$str = $str."<key>last_name</key><string>".$lastname."</string>";
	$str = $str."<key>password</key><string>".$password."</string>";
	$str = $str."<key>submit</key><string>Get Capabilities</string>";
	$str = $str."</map></llsd>";
	
	$dom = regapiHttpPostDOM( URI_GET_CAPABILITES, $str );
	if( regapiProcessDOMForErrors($dom) )
		return false;
	return $dom;
}
 
// Check to see if <username> (first name) and <last_name_id> (an integer code
//	as returned from regapiGetLastNames below) are a valid pair to create a
//	new account with.  If they are, the function returns true.  If not, it
//	returns false.  It also returns false if there was a query error, but you
//	can check to see if regapiGetCurrentErrors() returns anything to see if
//	there was an error or not.
function	regapiCheckName( $username, $last_name_id )
{
	// Yea, maybe this is kinda lame, but seriously easier than constructing
	//	a DOM object and ->saveXML()ing. >_>
 
	// Build the XML.
	$str = "<llsd><map><key>username</key><string>".$username.
			"</string><key>last_name_id</key><integer>".$last_name_id.
			"</integer></map></llsd>";
 
	// POST it and get a DOM object back.
	$dom = regapiHttpPostDOM( URI_CHECK_NAME, $str );
	if( regapiProcessDOMForErrors($dom) )
		return false;
 
	// Parse out the return value and return boolean.
	$nodelist = $dom->getElementsByTagName( "boolean" );
	if( $nodelist != NULL && $nodelist->length )
		return $nodelist->item(0)->textContent == 'true';
	regapiSetUnknownError();
	return false;
}
 
// Create a new user, using the required fields <username> (first name),
//	<last_name_id> (an integer code as returned from regapiGetLastNames below),
//	<email>, and <password>.  <options> is an associative array of optional
//	parameters (see https://wiki.secondlife.com/wiki/Registration_API_Reference#Parameters_2),
//	where the key is the option name and the value is the, uh, value of that
//	option.  The function returns the agent's UUID.
// Returns NULL on error, then you can access the error codes with
//	regapiGetCurrentErrors().
function	regapiCreateUser( $username, $last_name_id, $email, $password, $dob, $options )
{
	// A mapping of options and their types, used in verification and XML
	//	creation.
	$valid_options = array(
			"username"			=> "string",
			"last_name_id"		=> "integer",
			"email"				=> "string",
			"password"			=> "string",
			"dob"				=> "string",
			"limited_to_estate"	=> "integer",
			"start_region_name"	=> "string",
			"start_local_x"		=> "float",
			"start_local_y"		=> "float",
			"start_local_z"		=> "float",
			"start_look_at_x"	=> "float",
			"start_look_at_y"	=> "float"
		);
 
	// Remove any invalid optional, uh, options.
	if( !is_array($options) )
		$options = array();
	foreach( $options as $opt => $val )
	{
		if( !array_key_exists($opt,$valid_options) )
		{
			?><b>Warning</b>:  Unrecognized option <b><?php echo $opt; ?></b> removed from regapiCreateUser list.<br /><?php
			unset( $options[$opt] );
		}
	}
 
	// Add the required options to the option list.
	$options["username"]		= $username;
	$options["last_name_id"]	= $last_name_id;
	$options["email"]			= $email;
	$options["password"]		= $password;
	$options["dob"]				= $dob;
 
	// Build the XML.
	// Yea, maybe this is kinda lame, but seriously easier than constructing
	//	a DOM object and ->saveXML()ing. >_>
	$str = "<llsd><map>";
	foreach( $options as $opt => $val )
	{
		$type = $valid_options[$opt];
		$str = $str."<key>".$opt."</key><".$type.">".$val."</".$type.">";
	}
	$str = $str."</map></llsd>";
 
	// POST it and get a DOM object back.
	$dom = regapiHttpPostDOM( URI_CREATE_USER, $str );
	if( regapiProcessDOMForErrors($dom) )
		return NULL;
 
	// Parse out the return value and return agent uuid string.
	$nodelist = $dom->getElementsByTagName( "string" );
	if( $nodelist != NULL && $nodelist->length )
		return $nodelist->item(0)->textContent;
	regapiSetUnknownError();
	return NULL;
}
 
// This function returns an associative 2d array of error codes, with the first
//	key being the integer code, and the second key being either "desc" for the
//	short error description, or "message" for the verbose error message. See
//	the example index.php file for example of usage.
// Returns NULL on error, then you can access the error codes with
//	regapiGetCurrentErrors(), although you won't be able to look up the error
//	code text, lol.
function regapiGetErrorCodes()
{
	global $regAPI_errorcodecache;
 
	// We cache the first query for error codes so that subsequent calls to not
	//	cause more queries.
	if( $regAPI_errorcodecache != NULL )
		return $regAPI_errorcodecache;
 
	$errors = array();
 
	$dom = regapiHttpGetDOM( URI_GET_ERROR_CODES );
	if( regapiProcessDOMForErrors($dom) )
		return NULL;
 
	// Get <llsd>.
	$node = $dom->documentElement;
	// Move to enclosing <array>.
	if( $node != NULL )
		$node = $node->firstChild;
	// move to first sub-<array>.
	if( $node != NULL && $node->nodeName == "array" )
		$node = $node->firstChild;
	else
	{
		regapiSetUnknownError();
		return NULL;
	}
	// Move through sub-<array>s and build the error list.
	while( $node != NULL && $node->nodeName == "array" )
	{
		// Build the array from the sub-<array> children.
		$child = $node->firstChild;
		// Get code.
		if( $child != NULL )
		{
			$code = $child->textContent;
			$child = $child->nextSibling;
		}
		else
		{
			regapiSetUnknownError();
			return NULL;
		}
		// Get short text.
		if( $child != NULL )
		{
			$desc = $child->textContent;
			$child = $child->nextSibling;
		}
		else
		{
			regapiSetUnknownError();
			return NULL;
		}
		// Get verbose error message.
		if( $child != NULL )
			$message = $child->textContent;
		else
		{
			regapiSetUnknownError();
			return NULL;
		}
		$errors[$code]["desc"] = $desc;
		$errors[$code]["message"] = $message;
		$node = $node->nextSibling;
	}
 
	$regAPI_errorcodecache = $errors;
	return $errors;
}
 
// Returns an associative array of available last names, in alphabetical
//	order, where key is the name string and value is the numeric id
//	corresponding to that name.  If you pass <num> of 1 or more, it will
//	randomly choose that many names from the list of available names and return
//	them.  If <num> is negative, it will return all available names.  If you
//	pass something into the <username> parameter, only last names that are
//	available with the specified username (first name) will be returned.
//	WARNING: Passing something into <username> is currently very slow, and gets
//	even slower the larger <num> is...
// Returns NULL on error, then you can access the error codes with
//	regapiGetCurrentErrors().
function	regapiGetLastNames( $username, $num )
{
	$dom = regapiHttpGetDOM( URI_GET_LAST_NAMES );
	if( regapiProcessDOMForErrors($dom) )
		return NULL;
 
	$names = array();
 
	$nodelist = $dom->getElementsByTagName( "map" );
	if( $nodelist != NULL && $nodelist->length )
	{
		// This will only return the names in the first map, but there is
		//	only one map anyway.
		$node = $nodelist->item(0)->firstChild;
		while( $node != NULL )
		{
			$k = $node->textContent;
			$node = $node->nextSibling;
			if( $node == NULL_KEY )
			{
				// This should never happen.
				return rval;
			}
			$s = $node->textContent;
			$node = $node->nextSibling;
			$names[$s] = $k;
		}
	}
	$rval = array();
	while( ($num>=0&&count($rval)<$num) || ($num<0&&count($names)) )
	{
		if( $num < 0 )
		{
			$rval = $names;
			$names = array();
		}
		else
		{
			$entries = array_rand( $names, $num-count($rval) );
			foreach( $entries as $k )
			{
				if( $username=="" || $username==NULL || regapiCheckName($username,$names[$k]) )
					$rval[$k] = $names[$k];
				unset( $names[$k] );
			}
		}
		if( !count($names) )
			break;
	}
	ksort( $rval );
	return $rval;
}
 
function	regapiGetCurrentErrors()
{
	global $regAPI_lasterrors;
	if( !count($regAPI_lasterrors) )
		return NULL;
	return $regAPI_lasterrors;
}
 
////////////////////////////////////////////////////////////////////////////////
// Internal things below here. You probably don't need to worry about them.
////////////////////////////////////////////////////////////////////////////////
 
$regAPI_lasterrors		= array();
$regAPI_errorcodecache	= NULL;
 
// Utility function for random errors not reported via XML returns from queries.
//	We just set a 0 (undefined) error.
function	regapiSetUnknownError()
{
	global $regAPI_lasterrors;
	$regAPI_lasterrors = array();
	$regAPI_lasterrors[] = 0;
}
 
// This function re-sets the lasterrors array, and returns true if the XML
//	looks like an error message, false otherwise.
function	regapiProcessDOMForErrors( $dom )
{
	global $regAPI_lasterrors;
 
	$regAPI_lasterrors = array();
 
	if( $dom == NULL )
		return true;
	// Get <llsd>.
	$node = $dom->documentElement;
	// Move to enclosing <array>.
	if( $node != NULL )
		$node = $node->firstChild;
	// Move to first <integer> node, if we're really in an <array>.
	if( $node != NULL && $node->nodeName == "array" )
		$node = $node->firstChild;
	else
		return false;
	$startnode = $node;
	// Scan over child nodes to see if they are all integers.
	while( $node != NULL )
	{
		if( $node->nodeName != "integer" )
			return false;
		$node = $node->nextSibling;
	}
	// If we get here, then the array is all integers, and it looks like an
	//	error message. So we add the error codes to the array.
	$node = $startnode;
	while( $node != NULL )
	{
		$regAPI_lasterrors[] = $node->textContent;
		$node = $node->nextSibling;
	}
	return true;
}
 
// Send a GET request to <url> and return a DOM object, or NULL on error.
function	regapiHttpGetDOM( $url )
{
	$ch = curl_init( $url );
	curl_setopt( $ch, CURLOPT_RETURNTRANSFER, TRUE );
    curl_setopt( $ch, CURLOPT_SSL_VERIFYPEER, FALSE );
	$doc = curl_exec( $ch );
	$status = curl_getinfo( $ch, CURLINFO_HTTP_CODE );
	curl_close( $ch );
	if( $status >= 400 )
		return NULL;
 
	$dom = new DOMDocument();
	if( !(@$dom->loadXML($doc)) )
		return NULL;
	return $dom;
}
 
// Send a POST request to <url> and return a DOM object, or NULL on error.
function	regapiHttpPostDOM( $url, $str )
{
	$ch = curl_init( $url );
    curl_setopt( $ch, CURLOPT_SSL_VERIFYPEER, FALSE );
	curl_setopt( $ch, CURLOPT_RETURNTRANSFER, TRUE );
	curl_setopt( $ch, CURLOPT_POST, TRUE );
	curl_setopt( $ch, CURLOPT_FAILONERROR, 1 );
	curl_setopt( $ch, CURLOPT_FOLLOWLOCATION, 1 );
	curl_setopt( $ch, CURLOPT_HTTPHEADER, Array("Content-Type: application/xml") );
	curl_setopt( $ch, CURLOPT_POSTFIELDS, $str );
	$doc = curl_exec( $ch );
	$status = curl_getinfo( $ch, CURLINFO_HTTP_CODE );
	curl_close( $ch );
	if( $status >= 400 )
		return NULL;
 
	$dom = new DOMDocument();
	if( !(@$dom->loadXML($doc)) )
		return NULL;
	return $dom;
}
 
?>