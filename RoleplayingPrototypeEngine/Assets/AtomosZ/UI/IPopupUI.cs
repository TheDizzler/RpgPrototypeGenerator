namespace AtomosZ.UI
{
	public interface IPopupUI
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="timeToOpen">Optional: Time in seconds for opening animation to finish.</param>
		void Show(bool skipAnimation = false);
		/// <summary>
		/// 
		/// </summary>
		/// <param name="timeToClose">Optional: Time in seconds for closing animation to finish.</param>
		void Hide(bool skipAnimation = false);
	}
}