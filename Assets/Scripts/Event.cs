using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public class Event : System.Object
{
    public string name;
    public string defaultNextEvent;
    public Sprite background;
    public Dialogue[] dialogues;
    public Decision[] decisions;
}