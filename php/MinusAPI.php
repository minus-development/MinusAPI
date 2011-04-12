<?php

/**
 * MinusAPI class, a simple cURL-based PHP wrapper for the min.us API.
 * Feel free to use and modify.
 *
 * @author Rodrigo Ferreira <rodrigo.ferreira@gmail.com>
 */
class MinusAPI
{
	private $_cookies;

	public function __construct()
	{
		$this->_cookies=tempnam(sys_get_temp_dir(),'minus');
		if($this->_cookies===false)
			throw new Exception('Error creating temporary file.');
	}

	protected function call($service,$postfields=array())
	{
		$ch=curl_init();
		curl_setopt($ch,CURLOPT_URL,'http://min.us/api/'.$service);
		curl_setopt($ch,CURLOPT_COOKIEJAR,$this->_cookies);
		curl_setopt($ch,CURLOPT_COOKIEFILE,$this->_cookies);
		curl_setopt($ch,CURLOPT_HEADER,false);
		curl_setopt($ch,CURLOPT_RETURNTRANSFER,true);
		curl_setopt($ch,CURLOPT_VERBOSE,false);
		if(count($postfields)>0)
		{
			curl_setopt($ch,CURLOPT_POST,true);
			curl_setopt($ch,CURLOPT_POSTFIELDS,$postfields);
		}
		$retval=curl_exec($ch);
		$errno=curl_errno($ch);
		$error=curl_error($ch);
		curl_close($ch);
		if($errno!=0)
			throw new Exception($error);
		return json_decode($retval);
	}

	public function CreateGallery()
	{
		return $this->call(__FUNCTION__);
	}

	public function GetItems($id)
	{
		return $this->call(__FUNCTION__.'/m'.$id);
	}

	public function MyGalleries()
	{
		return $this->call(__FUNCTION__.'.json');
	}

	public function SaveGallery($id,$name,$items,$key='OK')
	{
		return $this->call(__FUNCTION__,array(
			'id'=>$id,
			'name'=>$name,
			'key'=>$key,
			'items'=>json_encode($items),
		));
	}

	public function SignIn($username,$password)
	{
		return $this->call(__FUNCTION__,array(
			'username'=>$username,
			'password1'=>$password,
		));
	}

	public function SignOut()
	{
		return $this->call(__FUNCTION__);
	}

	public function UploadItem($id,$name,$path,$key='OK')
	{
		return $this->call(__FUNCTION__.'?'.http_build_query(array(
			'editor_id'=>$id,
			'filename'=>$name,
			'key'=>$key,
		)),array(
			'file'=>"@{$path}",
		));
	}
}
