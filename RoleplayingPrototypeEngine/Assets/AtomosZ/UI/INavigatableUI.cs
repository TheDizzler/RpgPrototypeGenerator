namespace AtomosZ.UI
{
	public interface INavigatableUI
	{
		void NavigateUp();
		void NavigateDown();
		void NavigateRight();
		void NavigateLeft();
		bool Confirm();
		void Cancel();
	}
}