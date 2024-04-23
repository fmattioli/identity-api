﻿using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SpendManagement.Identity.API.Controllers;
using SpendManagement.Identity.Application.Requests;
using SpendManagement.Identity.Application.Responses;
using SpendManagement.Identity.Application.Services;
using System.Security.Claims;

namespace Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<HttpContext> _httpContext;
        private readonly UserController _userController;
        private readonly Mock<IIdentityService> _identityServiceMock = new();
        private readonly Fixture _fixture = new();

        public UserControllerTests()
        {
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "userIdValue"),
            }));

            _httpContext = new Mock<HttpContext>();

            _httpContext
                .Setup(c => c.User)
                .Returns(claimsPrincipal);

            _userController = new UserController(_identityServiceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _httpContext.Object
                }
            };
        }

        [Fact]
        public async Task GivenValidSignUpOnSignUpEndpoint_SignUpMethodShouldBeCalled()
        {
            //Arrange
            var userSignUp = _fixture.Create<SignUpUserRequest>();
            var userResponse = _fixture
                .Build<UserResponse>()
                .With(x => x.Success, true)
                .Create();

            _identityServiceMock
                .Setup(x => x.SignUpAsync(It.IsAny<SignUpUserRequest>()))
                .ReturnsAsync(userResponse);

            _identityServiceMock
                .Setup(x => x.AddUserInClaimAsync(It.IsAny<AddUserInClaimRequest>()))
                .Returns(Task.FromResult(It.IsAny<UserResponse>()));

            //Act
            await _userController.SignUpAsync(userSignUp);

            //Assert
            _identityServiceMock.Verify(x => x.SignUpAsync(userSignUp), Times.Once);
            _identityServiceMock.Verify(x => x.AddUserInClaimAsync(It.IsAny<AddUserInClaimRequest>()), Times.Once);
            _identityServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GivenValidLoginOnLoginEndpoint_SignInMethodShouldBeCalled()
        {
            //Arrange
            var userSignIn = _fixture.Create<SignInUserRequest>();
            var userResponse = _fixture.Create<UserLoginResponse>();

            _identityServiceMock
                .Setup(x => x.LoginAsync(userSignIn))
                .Returns(Task.FromResult(userResponse));

            //Act
            await _userController.MakeLoginAsync(userSignIn);

            //Assert
            _identityServiceMock.Verify(x => x.LoginAsync(userSignIn), Times.Once);
            _identityServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GivenValidEmailOnGetUserClaimsEndpoint_GetUserClaimsAsyncShouldBeCalled()
        {
            //Arrange
            var email = _fixture.Create<string>();
            var claim1 = new Claim(_fixture.Create<string>(), _fixture.Create<string>());
            var claim2 = new Claim(_fixture.Create<string>(), _fixture.Create<string>());

            var claims = new List<Claim> { claim1, claim2 };

            _identityServiceMock
                .Setup(x => x.GetUserClaimsAsync(email))
                .Returns(Task.FromResult(claims.Select(x => x)));

            //Act
            await _userController.GetUserClaimsAsync(email);

            //Assert
            _identityServiceMock.Verify(x => x.GetUserClaimsAsync(email), Times.Once);
            _identityServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GivenValidUserClaimOnAddUserInClaimEndpoint_AddUserInClaimAsyncShouldBeCalled()
        {
            //Arrange
            var userClaim = _fixture.Create<AddUserInClaimRequest>();
            var userResponse = _fixture.Create<UserResponse>();

            _identityServiceMock
                .Setup(x => x.AddUserInClaimAsync(userClaim))
                .Returns(Task.FromResult(userResponse));

            //Act
            await _userController.AddUserInClaimAsync(userClaim);

            //Assert
            _identityServiceMock.Verify(x => x.AddUserInClaimAsync(userClaim), Times.Once);
            _identityServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GivenValidRefreshLoginOnRefreshLoginEndpoint_RefreshLoginShouldBeCalled()
        {
            //Arrange
            var identity = _httpContext.Object.User.Identity as ClaimsIdentity;
            var userId = identity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userResponse = _fixture.Create<UserLoginResponse>();

            _identityServiceMock
                .Setup(x => x.LoginWithoutPasswordAsync(userId!))
                .Returns(Task.FromResult(userResponse));

            //Act
            await _userController.RefreshLoginAsync();

            //Assert
            _identityServiceMock.Verify(x => x.LoginWithoutPasswordAsync(userId!), Times.Once);
            _identityServiceMock.VerifyNoOtherCalls();
        }
    }
}
