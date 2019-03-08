using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;


namespace Aquacloud.Data
{
	class FileEntryStore : INoteEntryStore
	{
		List<NoteEntry> loadedNotes;
		string filename;

		public FileEntryStore()
		{
			string folder = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);
			if (string.IsNullOrEmpty(folder))
				folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			this.filename = Path.Combine(folder, "minutes2.xml");
		}

		private async Task InitializeAsync()
		{
			if (loadedNotes == null)
			{
				loadedNotes = (await ReadDataAsync(filename)).ToList();
			}
		}

		public async Task AddAsync(NoteEntry entry)
		{
			await InitializeAsync();

			if (!loadedNotes.Any(ne => ne.Id == entry.Id))
			{
				loadedNotes.Add(entry);
				await SaveDataAsync(filename, loadedNotes);
			}
		}

		public async Task DeleteAsync(NoteEntry entry)
		{
			await InitializeAsync();

			if (loadedNotes.Remove(entry))
			{
				await SaveDataAsync(filename, loadedNotes);
			}
		}

		public async Task<IEnumerable<NoteEntry>> GetAllAsync()
		{
			await InitializeAsync();
			return loadedNotes.OrderByDescending(n => n.CreatedDate);
		}

		public async Task<NoteEntry> GetByIdAsync(string id)
		{
			await InitializeAsync();
			return loadedNotes.SingleOrDefault(n => n.Id == id);
		}

		public async Task UpdateAsync(NoteEntry entry)
		{
			await InitializeAsync();

			if (!loadedNotes.Contains(entry))
			{
				throw new Exception($"NoteEntry {entry.Title} was not found in the {nameof(FileEntryStore)}. Did you forget to add it?");
			}

			await SaveDataAsync(filename, loadedNotes);
		}

		private static async Task<IEnumerable<NoteEntry>> ReadDataAsync(string filename)
		{
			if (!File.Exists(filename))
			{
				return Enumerable.Empty<NoteEntry>();
			}

			string text;
			using (var reader = new StreamReader(filename))
			{
				text = await reader.ReadToEndAsync().ConfigureAwait(false);
			}

			if (string.IsNullOrWhiteSpace(text))
			{
				return Enumerable.Empty<NoteEntry>();
			}

			IEnumerable<NoteEntry> result = XDocument.Parse(text)
							.Root
							.Elements("entry")
							.Select(e =>
									new NoteEntry
									{
										Title = e.Attribute("title").Value,
										Text = e.Attribute("text").Value,
										CreatedDate = (DateTime)e.Attribute("createdDate")
									});

			return result;
		}

		static async Task SaveDataAsync(string filename, IEnumerable<NoteEntry> notes)
		{
			XDocument root = new XDocument(
					new XElement("minutes",
							notes.Select(n =>
									new XElement("entry",
											new XAttribute("title", n.Title ?? ""),
											new XAttribute("text", n.Text ?? ""),
											new XAttribute("createdDate", n.CreatedDate)))));

			using (StreamWriter writer = new StreamWriter(filename))
			{
				await writer.WriteAsync(root.ToString()).ConfigureAwait(false);
			}

			CloudStorageAccount storageAccount = null;
			CloudBlobContainer cloudBlobContainer = null;

			//string storageConnectionString = Environment.GetEnvironmentVariable("storageconnectionstring");
			string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=centralupload;AccountKey=ojtXJVDJNNveH3knEvOh1ZLAKnTyhsQZLaSbbB0XmvMSHD4xJUAzuLZCXjlBr05aoVzYix4EQUdaMHx+W4fCAg==;EndpointSuffix=core.windows.net";

			// Check whether the connection string can be parsed.
			if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
			{
				try
				{
					// Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
					CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

					// Create a container called 'quickstartblobs' and append a GUID value to it to make the name unique. 
					cloudBlobContainer = cloudBlobClient.GetContainerReference("documents");
					//await cloudBlobContainer.CreateAsync();

					// Set the permissions so the blobs are public. 
					BlobContainerPermissions permissions = new BlobContainerPermissions
					{
						PublicAccess = BlobContainerPublicAccessType.Blob
					};
					await cloudBlobContainer.SetPermissionsAsync(permissions);

					string localFileName = "QuickStart_" + Guid.NewGuid().ToString() + ".txt";

					// Get a reference to the blob address, then upload the file to the blob.
					// Use the value of localFileName for the blob name.
					CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(localFileName);
					await cloudBlockBlob.UploadFromFileAsync(filename);

				}
				catch (StorageException ex)
				{
					Console.WriteLine("Error returned from the service: {0}", ex.Message);
				}
			}
		}
	}
} 
