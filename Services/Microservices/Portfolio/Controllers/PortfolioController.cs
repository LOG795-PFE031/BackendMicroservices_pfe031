﻿using AuthNuget.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Commands.Seedwork;
using Portfolio.Commands.ShareVolume;
using Portfolio.Domain.Monads;
using Portfolio.Domain.ValueObjects;
using Portfolio.Queries.Price;
using Portfolio.Queries.Seedwork;
using Portfolio.Queries.ShareVolume;
using Portfolio.Queries.Time;
using Portfolio.Queries.User;

namespace Portfolio.Controllers;

// [Authorize(Roles = $"{RoleConstants.Client}, {RoleConstants.AdminRole}")]
[ApiController]
[Route("portfolio")]
public sealed class PortfolioController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ShareVolumesViewModel>> GetShareVolumes(IQueryDispatcher queryDispatcher)
    {
        var user = User.GetUsername();

        var getUserWalletId = new GetUserWalletId(user);

        Result<string> walletIdResult = await queryDispatcher.DispatchAsync<GetUserWalletId, string>(getUserWalletId);

        if (walletIdResult.IsFailure()) return NotFound(walletIdResult.Exception!.Message);

        var query = new GetSharesVolume(walletIdResult.Content!);

        Result<ShareVolumesViewModel> result = await queryDispatcher.DispatchAsync<GetSharesVolume, ShareVolumesViewModel>(query);

        return result.IsSuccess() ? Ok(result.Content) : BadRequest(result.Exception!.Message);
    }

    [HttpPatch("sell/{symbol}/{volume}")]
    public async Task<ActionResult> SellStock(string symbol, int volume, ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
    {
        if(volume <= 0) return BadRequest("Volume must be positive");

        return await ModifyShareVolume(symbol, -volume, commandDispatcher, queryDispatcher);
    }

    [HttpPatch("buy/{symbol}/{volume}")]
    public async Task<ActionResult> BuyStock(string symbol, int volume, ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
    {
        if (volume <= 0) return BadRequest("Volume must be positive");

        return await ModifyShareVolume(symbol, volume, commandDispatcher, queryDispatcher);
    }

    private async Task<ActionResult> ModifyShareVolume(string symbol, int volume, ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
    {
        var user = User.GetUsername();

        var getUserWalletId = new GetUserWalletId(user);

        Result<string> walletIdResult = await queryDispatcher.DispatchAsync<GetUserWalletId, string>(getUserWalletId);

        if (walletIdResult.IsFailure()) return NotFound(walletIdResult.Exception!.Message);

        var getCurrentTime = new GetCurrentTime();

        Result<DateTime> currentTimeResult = await queryDispatcher.DispatchAsync<GetCurrentTime, DateTime>(getCurrentTime);

        if (currentTimeResult.IsFailure()) return BadRequest(currentTimeResult.Exception!.Message);

        var getStockPrice = new GetStockPrice(symbol, currentTimeResult.Content);

        Result<decimal> stockPriceResult = await queryDispatcher.DispatchAsync<GetStockPrice, decimal>(getStockPrice);

        if (stockPriceResult.IsFailure()) return BadRequest(stockPriceResult.Exception!.Message);

        var command = new ModifyShareVolume(symbol, new Money(stockPriceResult.Content), volume, walletIdResult.Content!);

        Result commandResult = await commandDispatcher.DispatchAsync(command);

        return commandResult.IsSuccess() ? Ok() : BadRequest(commandResult.Exception!.Message);
    }
}