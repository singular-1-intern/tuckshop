namespace Tuckshop.AuthorisationServer.Models
{
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using Neo.Options;

  /// <summary>
  /// User enrolment options.
  /// </summary>
  [ConfigSectionName("AuthorisationServer:Enrolment")]
  public class UserEnrolmentHandlerOptions : ValidateableOptions<UserEnrolmentHandlerOptions>
  {
    /// <summary>
    /// Gets or sets the list of administrator usernames. These usernames will be automatically added to the admins group on enrolment.
    /// </summary>
    [Required]
    public List<string>? Administrators { get; set; }
  }
}