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
			_changeTable = new ChangeTable(doc);
		}

		private ChangeTable _changeTable;

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
			await _changeTable.UpdateChangeAsync(initialChange);

			var updatedChange = new TextChange(id, "Hello", "Everyone");

			// Act
			await _changeTable.UpdateChangeAsync(updatedChange);
			var result = _changeTable.TryGetValue<TextChange>(id, out var retrievedChange);

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
			_changeTable.UpdateChangeAsync(change).Wait();
			var result = _changeTable.TryGetValue<TextChange>(id, out var retrievedChange);

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
			_changeTable.UpdateChangeAsync(change).Wait();

			// Act
			_changeTable.RemoveChange(id);
			var result = _changeTable.TryGetValue<TextChange>(id, out var retrievedChange);

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
			_changeTable.UpdateChangeAsync(change1).Wait();
			_changeTable.UpdateChangeAsync(change2).Wait();

			// Act
			_changeTable.RemoveChanges(new[] { change1, change2 });
			var result1 = _changeTable.TryGetValue<TextChange>(id1, out var retrievedChange1);
			var result2 = _changeTable.TryGetValue<TextChange>(id2, out var retrievedChange2);

			// Assert
			Assert.IsFalse(result1);
			Assert.IsNull(retrievedChange1);
			Assert.IsFalse(result2);
			Assert.IsNull(retrievedChange2);
		}
	}
}
