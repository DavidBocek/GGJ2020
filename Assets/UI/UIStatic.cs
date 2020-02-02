using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UIStatic
{
	//======================================================================
	// Constants
	//======================================================================
	public static string MONEY = "money";
	public static string CUR_CUBOS = "cur_cubos";
	public static string CUR_SACRIFICE = "cur_nega";
	public static string MAX_SACRIFICE = "max_nega";

	//======================================================================
	// Dictionaries
	//======================================================================
	private static Dictionary<string, int> m_intVars;
	private static Dictionary<string, float> m_floatVars;
	private static Dictionary<string, bool> m_boolVars;
	private static Dictionary<string, string> m_stringVars;

	public static void Init()
	{
		m_intVars = new Dictionary<string, int>();
		m_floatVars = new Dictionary<string, float>();
		m_boolVars = new Dictionary<string, bool>();
		m_stringVars = new Dictionary<string, string>();
	}

	//=== ints ===
	public static bool HasInt(string var)
	{
		return m_intVars.ContainsKey(var);
	}
	public static void SetInt(string var, int value)
	{
		if (!m_intVars.ContainsKey(var))
		{
			m_intVars.Add(var, value);
		}
		else
		{
			m_intVars[var] = value;
		}
	}
	public static int GetInt(string var)
	{
		return m_intVars[var];
	}

	//=== floats ===
	public static bool HasFloat(string var)
	{
		return m_floatVars.ContainsKey(var);
	}
	public static void SetFloat(string var, float value)
	{
		if (!m_floatVars.ContainsKey(var))
		{
			m_floatVars.Add(var, value);
		}
		else
		{
			m_floatVars[var] = value;
		}
	}
	public static float GetFloat(string var)
	{
		return m_floatVars[var];
	}

	//=== bools ===
	public static bool HasBool(string var)
	{
		return m_boolVars.ContainsKey(var);
	}
	public static void SetBool(string var, bool value)
	{
		if (!m_boolVars.ContainsKey(var))
		{
			m_boolVars.Add(var, value);
		}
		else
		{
			m_boolVars[var] = value;
		}
	}
	public static bool GetBool(string var)
	{
		return m_boolVars[var];
	}

	//=== strings ===
	public static bool HasString(string var)
	{
		return m_stringVars.ContainsKey(var);
	}
	public static void SetString(string var, string value)
	{
		if (!m_stringVars.ContainsKey(var))
		{
			m_stringVars.Add(var, value);
		}
		else
		{
			m_stringVars[var] = value;
		}
	}
	public static string GetString(string var)
	{
		return m_stringVars[var];
	}
}
