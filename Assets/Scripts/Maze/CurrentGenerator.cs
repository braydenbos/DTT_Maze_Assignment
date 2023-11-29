using UnityEngine;

namespace Maze
{
    public sealed class CurrentGenerator : MonoBehaviour
    {
        public Coroutine CurrentGeneration { get; set; }
    }
}
