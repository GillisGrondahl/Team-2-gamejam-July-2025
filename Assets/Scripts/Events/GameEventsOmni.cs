using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameEventsOmni : MonoBehaviour
    {
        public static GameEventsOmni instance;

        private void Awake()
        {
            instance = this;
        }

        public event Action OnStartGamePressed;

        public void StartGamePressed()
        {
            if (OnStartGamePressed != null) OnStartGamePressed();
        }



    }
}
