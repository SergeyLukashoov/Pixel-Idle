using System.Collections;
using System.Collections.Generic;
using Game.Player;
using UnityEngine;

namespace Core.Controllers.Save
{
    public interface ISaveController
    {
        public void Initialize(IPlayer player);
        public void Save();
        public void Load();
    }   
}
