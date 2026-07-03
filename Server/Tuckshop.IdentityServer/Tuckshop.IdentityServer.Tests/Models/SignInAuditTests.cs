namespace Tuckshop.IdentityServer.Tests.Models
{
  using Neo.IdentityServer.Models.SignIn;
  using Neo.Model.ValueObjects;
  using Xunit;

  public class SignInAuditTest
  {
    private readonly RequestHeaderDetails requestHeaderDetails;
    private readonly string UserID;
    public SignInAuditTest()
    {
      string userAgent = "testAgent";
      string LocalHostIP = "127.0.0.1";
      this.requestHeaderDetails = RequestHeaderDetails.Create(userAgent, LocalHostIP);
      this.UserID = "SignInAudit";
    }

    [Fact]
    public void SuccessSignInAudit_WithNoDetail_Expect_SignInAuditObject()
    {
      // Arrange
      var ExpectedUserId = this.UserID;

      // Act
      var signInAudit = SignInAudit.SuccessSignInAudit(this.UserID, identityProviderId: 1, this.requestHeaderDetails);

      // Assert
      Assert.True(signInAudit.Success);
      Assert.Equal(ExpectedUserId, signInAudit.UserId);
      Assert.IsType<RequestHeaderDetails>(signInAudit.Request);
      Assert.Equal(this.requestHeaderDetails, signInAudit.Request);
    }

    [Fact]
    public void SuccessSignInAudit_WithDetail_Expect_SignInAuditObject()
    {
      // Arrange
      var ExpectedUserId = this.UserID;
      var auditDetails = "Some audit details for testing";

      // Act
      var signInAudit = SignInAudit.SuccessSignInAudit(this.UserID, identityProviderId: 1, auditDetails, this.requestHeaderDetails);

      // Assert
      Assert.True(signInAudit.Success);
      Assert.Equal(ExpectedUserId, signInAudit.UserId);
      Assert.Equal(auditDetails, signInAudit.Details);
      Assert.IsType<RequestHeaderDetails>(signInAudit.Request);
      Assert.Equal(this.requestHeaderDetails, signInAudit.Request);
    }

    [Fact]
    public void FailedSignInAudit_WithDetail_Expect_SignInAuditObject()
    {
      // Arrange
      var ExpectedUserId = this.UserID;
      var auditDetails = "Some audit details for testing";

      // Act
      var signInAudit = SignInAudit.FailedSignInAudit(this.UserID, identityProviderId: 1, auditDetails, this.requestHeaderDetails);

      // Assert
      Assert.False(signInAudit.Success);
      Assert.Equal(ExpectedUserId, signInAudit.UserId);
      Assert.Equal(auditDetails, signInAudit.Details);
      Assert.IsType<RequestHeaderDetails>(signInAudit.Request);
      Assert.Equal(this.requestHeaderDetails, signInAudit.Request);
    }
  }
}
