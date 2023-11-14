using Crash.Changes;
using Crash.Common.Document;
using Crash.Common.Tables;

namespace Crash.Common.Tests.Tables
{
	[TestFixture]
	public class ChangeTableTests
	{
		[SetUp]
		public void SetUp()
		{
			var doc = new CrashDoc();
			_temporaryChangeTable = new TemporaryChangeTable(doc);
		}

		private TemporaryChangeTable _temporaryChangeTable;

		internal sealed class TextChange : IChange
		{
			public TextChange(Guid id, string owner, string newValue)
			{
				Id = id;
				NewValue = newValue;
				Owner = owner;
			}

			internal string NewValue { get; }

			public DateTime Stamp { get; }
			public Guid Id { get; }

			public string Owner { get; }
			public string? Payload { get; }
			public string Type { get; }
			public ChangeAction Action { get; set; }
		}

		[Test]
		public async Task UpdateChangeAsync_UpdatesCache_WhenCacheContainsChange()
		{
			// Arrange
			var id = Guid.NewGuid();
			var initialChange = new TextChange(id, "Hello", "World");
			_temporaryChangeTable.UpdateChange(initialChange);

			var updatedChange = new TextChange(id, "Hello", "Everyone");

			// Act
			_temporaryChangeTable.UpdateChange(updatedChange);
			var result = _temporaryChangeTable.TryGetChangeOfType<TextChange>(id, out var retrievedChange);

			// Assert
			Assert.That(result, Is.True);
			Assert.AreEqual("Everyone", retrievedChange.NewValue);
		}

		[Test]
		public void UpdateChangeAsync_AddsChange_WhenCacheDoesNotContainChange()
		{
			// Arrange
			var id = Guid.NewGuid();
			var change = new TextChange(id, "Hello", "World");

			// Act
			_temporaryChangeTable.UpdateChange(change);
			var result = _temporaryChangeTable.TryGetChangeOfType<TextChange>(id, out var retrievedChange);

			// Assert
			Assert.That(result, Is.True);
			Assert.AreEqual("World", retrievedChange.NewValue);
		}

		[Test]
		public void RemoveChange_RemovesChangeFromCache()
		{
			// Arrange
			var id = Guid.NewGuid();
			var change = new TextChange(id, "Hello", "World");
			_temporaryChangeTable.UpdateChange(change);

			// Act
			_temporaryChangeTable.RemoveChange(id);
			var result = _temporaryChangeTable.TryGetChangeOfType<TextChange>(id, out var retrievedChange);

			// Assert
			Assert.IsFalse(result);
			Assert.IsNull(retrievedChange);
		}

		[Test]
		public void RemoveChanges_RemovesChangesFromCache()
		{
			// Arrange
			var id1 = Guid.NewGuid();
			var id2 = Guid.NewGuid();
			var change1 = new TextChange(id1, "Hello", "World");
			var change2 = new TextChange(id2, "Goodbye", "World");
			_temporaryChangeTable.UpdateChange(change1);
			_temporaryChangeTable.UpdateChange(change2);

			// Act
			_temporaryChangeTable.RemoveChanges(new[] { change1, change2 });
			var result1 = _temporaryChangeTable.TryGetChangeOfType<TextChange>(id1, out var retrievedChange1);
			var result2 = _temporaryChangeTable.TryGetChangeOfType<TextChange>(id2, out var retrievedChange2);

			// Assert
			Assert.IsFalse(result1);
			Assert.IsNull(retrievedChange1);
			Assert.IsFalse(result2);
			Assert.IsNull(retrievedChange2);
		}
	}
}
