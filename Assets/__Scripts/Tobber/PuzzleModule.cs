using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class PuzzleModule : MonoBehaviour
{
    public int[] inputPorts = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public int[] outputPorts = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public Action<int[]> OnOutputUpdate;

    public abstract void setInput(int[] input);
}
    
