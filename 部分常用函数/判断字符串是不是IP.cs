using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class 判断字符串是不是IP : MonoBehaviour
{
    string pattern = @"^(([1-9]\d?)|(1\d{2})|(2[01]\d)|(22[0-3]))(\.((1?\d\d?)|(2[04]/d)|(25[0-5]))){3}$";

    void IsIP(string str)
    {
        if (Regex.IsMatch(str, pattern))
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("输入的IP地址正确");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("输入的IP地址有误");
        }
    }

}
