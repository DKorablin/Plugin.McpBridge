using System.ComponentModel;
using McpBridge.Core.Data;

namespace McpBridge.Core
{
	public partial class Settings
	{
		private Boolean _listChangedPending = false;

		private void AiAgents_ListChanged(Object? sender, ListChangedEventArgs e)
		{
			if(this._listChangedPending)
				return;
			this._listChangedPending = true;

			SynchronizationContext.Current?.Post(_ =>
			{
				if(this._aiAgents == null || this._aiAgents.Count == 0)
				{
					this._aiAgents ??= new System.ComponentModel.BindingList<Data.AiAgentDto>();
					this._aiAgents.Add(new Data.AiAgentDto() { AssistantSystemPrompt = Data.AiAgentDto.Defaults.AssistantSystemPrompt, });
					this.AiAgentsJson = null;
				}
				else
				{
					using(MemoryStream stream = new MemoryStream())
					{
						AgentSerializer.WriteObject(stream, this._aiAgents.ToArray());
						stream.Seek(0, SeekOrigin.Begin);
						this.AiAgentsJson = System.Text.Encoding.UTF8.GetString(stream.ToArray());
					}
				}

				this._listChangedPending = false;
			}, null);
		}

		private void AiProviders_ListChanged(Object? sender, ListChangedEventArgs e)
		{
			if(this._listChangedPending)
				return;
			this._listChangedPending = true;

			SynchronizationContext.Current?.Post(_ =>
			{
				if(this._aiProviders == null || this._aiProviders.Count == 0)
					this.AiProvidersJson = null;
				else
				{
					Boolean morphed = false;
					for(Int32 i = 0; i < this._aiProviders.Count; i++)
					{
						AiProviderDto morphedItem = AiProviderDto.Morph(this._aiProviders[i]);
						if(!ReferenceEquals(morphedItem, this._aiProviders[i]))
						{
							this._aiProviders[i] = morphedItem;
							morphed = true;
						}
					}
					if(morphed)
						this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.AiProviders)));
					using(MemoryStream stream = new MemoryStream())
					{
						ProvidersSerializer.WriteObject(stream, this._aiProviders.ToArray());
						stream.Seek(0, SeekOrigin.Begin);
						this.AiProvidersJson = System.Text.Encoding.UTF8.GetString(stream.ToArray());
					}
				}

				this._listChangedPending = false;

				if(this._aiAgents != null && this._aiProviders != null)
				{
					HashSet<Guid> providerIds = this._aiProviders.Select(x => x.Id).ToHashSet();
					foreach(AiAgentDto agent in this._aiAgents)
					{
						if(agent.SelectedProviderId != null && !providerIds.Contains(agent.SelectedProviderId.Value))
							agent.SelectedProviderId = null;
						if(agent.EmbeddingProviderId != null && !providerIds.Contains(agent.EmbeddingProviderId.Value))
							agent.EmbeddingProviderId = null;
					}
				}
			}, null);
		}

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler? PropertyChanged;
		private Boolean SetField<T>(ref T field, T value, String propertyName)
		{
			if(field is Array a && value is Array b
				? a.Cast<Object>().SequenceEqual(b.Cast<Object>())
				: EqualityComparer<T>.Default.Equals(field, value))
				return false;

			field = value;
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			return true;
		}
		#endregion INotifyPropertyChanged
	}
}