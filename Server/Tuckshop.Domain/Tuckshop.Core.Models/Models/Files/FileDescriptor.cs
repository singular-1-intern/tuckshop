namespace Tuckshop.Core.Models.Files
{
  using Neo.Model.AuditTrail;
  using Neo.Model.FileStorage;
  using Neo.Model.ValueObjects;

  /// <summary>
  /// Entity that describes a file and points to it's location.
  /// </summary>
  public class FileDescriptor : FileDescriptorBase<FileDescriptor, int>, IAuditTrailValueObjectEntity
  {
    /// <inheritdoc />
    public AuditValues? Audit { get; set; } = new AuditValues();
  }
}