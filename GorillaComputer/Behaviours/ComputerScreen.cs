using GorillaComputer.Models;
using System;
using System.Text;
using UnityEngine;

namespace GorillaComputer.Behaviours
{
    public abstract class ComputerScreen : MonoBehaviour
    {
        public static event Action<ComputerScreen, string> UpdateScreenAction;

        public abstract string Title { get; }

        public abstract string Summary { get; }

        public virtual bool IsParentalLocked { get; } = false;

        public abstract string GetContent();

        public virtual void OnScreenShow()
        {

        }

        public virtual void ProcessScreen(KeyBinding key)
        {

        }

        /// <summary>
        /// Updates the computer screen based on the content provided from our GetContent method
        /// </summary>
        public void UpdateScreen()
        {
            UpdateScreenAction?.Invoke(this, GetContent());
        }

        /// <summary>
        /// Updates the computer screen based on a provided StringBuilder
        /// </summary>
        /// <param name="stringBuilder"></param>
        public void UpdateScreen(StringBuilder stringBuilder)
        {
            UpdateScreenAction?.Invoke(this, stringBuilder.ToString());
        }

        /// <summary>
        /// Updates the computer screen based on a provided string
        /// </summary>
        /// <param name="content"></param>
        public void UpdateScreen(string content)
        {
            UpdateScreenAction?.Invoke(this, content);
        }
    }
}
