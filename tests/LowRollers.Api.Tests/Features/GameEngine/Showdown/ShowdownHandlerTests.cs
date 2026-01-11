using LowRollers.Api.Domain.Evaluation;
using LowRollers.Api.Domain.Events;
using LowRollers.Api.Domain.Models;
using LowRollers.Api.Domain.Pots;
using LowRollers.Api.Domain.StateMachine;
using LowRollers.Api.Features.GameEngine.Showdown;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LowRollers.Api.Tests.Features.GameEngine.Showdown;

public class ShowdownHandlerTests
{
    private readonly IHandEvaluationService _evaluationService = new HandEvaluationService();
    private readonly IPotManager _potManager = new PotManager();
    private readonly IHandEventStore _eventStore;
    private readonly ILogger<ShowdownHandler> _logger;
    private readonly ShowdownHandler _handler;

    public ShowdownHandlerTests()
    {
        _eventStore = Substitute.For<IHandEventStore>();
        _eventStore.GetLastSequenceNumberAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(0);
        _eventStore.AppendAsync(Arg.Any<IHandEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _logger = Substitute.For<ILogger<ShowdownHandler>>();

        _handler = new ShowdownHandler(
            _evaluationService,
            _potManager,
            _eventStore,
            _logger);
    }

    #region Helper Methods

    private static Card C(Rank rank, Suit suit) => new(suit, rank);

    private static Table CreateTableWithPlayers(int playerCount, decimal potAmount = 100m)
    {
        var table = new Table
        {
            Id = Guid.NewGuid(),
            Name = "Test Table",
            SmallBlind = 1m,
            BigBlind = 2m,
            ButtonPosition = 1
        };

        for (int i = 0; i < playerCount; i++)
        {
            var player = Player.Create(
                Guid.NewGuid(),
                $"Player{i + 1}",
                i + 1,
                1000m);
            player.Status = PlayerStatus.Active;
            table.Players[player.Id] = player;
        }

        return table;
    }

    private static Table CreateTableWithSpecificSeats(int[] seatPositions, int buttonPosition)
    {
        var table = new Table
        {
            Id = Guid.NewGuid(),
            Name = "Test Table",
            SmallBlind = 1m,
            BigBlind = 2m,
            ButtonPosition = buttonPosition
        };

        for (int i = 0; i < seatPositions.Length; i++)
        {
            var player = Player.Create(
                Guid.NewGuid(),
                $"PlayerSeat{seatPositions[i]}",
                seatPositions[i],
                1000m);
            player.Status = PlayerStatus.Active;
            table.Players[player.Id] = player;
        }

        return table;
    }

    private static Hand CreateShowdownHand(Table table, decimal potAmount = 100m)
    {
        var playerIds = table.Players.Keys.ToList();
        var hand = Hand.Create(
            table.Id,
            1,
            table.ButtonPosition,  // Use table's button position
            2,
            3,
            1m,
            2m,
            playerIds);

        hand.Phase = HandPhase.Showdown;
        hand.Pots[0].Amount = potAmount;
        foreach (var playerId in playerIds)
        {
            hand.Pots[0].AddEligiblePlayer(playerId);
        }

        // Add community cards
        hand.CommunityCards.AddRange([
            C(Rank.Two, Suit.Clubs),
            C(Rank.Five, Suit.Diamonds),
            C(Rank.Eight, Suit.Hearts),
            C(Rank.Jack, Suit.Spades),
            C(Rank.King, Suit.Clubs)
        ]);

        table.CurrentHand = hand;
        return hand;
    }

    #endregion

    #region ExecuteShowdownAsync Tests

    [Fact]
    public async Task ExecuteShowdownAsync_NoActiveHand_ReturnsFailure()
    {
        var table = CreateTableWithPlayers(2);
        table.CurrentHand = null;

        var result = await _handler.ExecuteShowdownAsync(table);

        Assert.False(result.IsSuccess);
        Assert.Contains("No active hand", result.Error);
    }

    [Fact]
    public async Task ExecuteShowdownAsync_IncompleteCommunityCards_ReturnsFailure()
    {
        var table = CreateTableWithPlayers(2);
        var hand = CreateShowdownHand(table);
        hand.CommunityCards.RemoveAt(4); // Remove one card

        var result = await _handler.ExecuteShowdownAsync(table);

        Assert.False(result.IsSuccess);
        Assert.Contains("Community cards not fully dealt", result.Error);
    }

    [Fact]
    public async Task ExecuteShowdownAsync_SinglePlayerRemaining_AwardsWithoutShowdown()
    {
        var table = CreateTableWithPlayers(2);
        var hand = CreateShowdownHand(table);

        // Set hole cards for players
        var players = table.Players.Values.ToList();
        players[0].HoleCards = [C(Rank.Ace, Suit.Hearts), C(Rank.Ace, Suit.Diamonds)];
        players[0].Status = PlayerStatus.Active;
        players[1].Status = PlayerStatus.Folded; // One player folded

        var result = await _handler.ExecuteShowdownAsync(table);

        Assert.True(result.IsSuccess);
        Assert.Single(result.PlayerResults);
        Assert.Contains(players[0].Id, result.TotalWinnings.Keys);
        Assert.Equal(100m, result.TotalWinnings[players[0].Id]);
    }

    [Fact]
    public async Task ExecuteShowdownAsync_TwoPlayers_CorrectlyDeterminesWinner()
    {
        var table = CreateTableWithPlayers(2);
        var hand = CreateShowdownHand(table);

        var players = table.Players.Values.OrderBy(p => p.SeatPosition).ToList();
        // Player 1: Pair of Aces (better hand)
        var playerWithAces = players[0];
        playerWithAces.HoleCards = [C(Rank.Ace, Suit.Hearts), C(Rank.Ace, Suit.Diamonds)];
        // Player 2: Pair of Sixes (worse hand - no conflicts with board)
        var playerWithSixes = players[1];
        playerWithSixes.HoleCards = [C(Rank.Six, Suit.Hearts), C(Rank.Six, Suit.Spades)];

        var result = await _handler.ExecuteShowdownAsync(table);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.PlayerResults.Count);

        // Player with Aces should win
        var winner = result.PlayerResults.First(r => r.AmountWon > 0);
        Assert.Equal(playerWithAces.Id, winner.PlayerId);
        Assert.Equal(100m, winner.AmountWon);
    }

    [Fact]
    public async Task ExecuteShowdownAsync_SplitPot_DividesEvenly()
    {
        var table = CreateTableWithPlayers(2);
        var hand = CreateShowdownHand(table);

        var players = table.Players.Values.ToList();
        // Both players have same hand (pair of Kings from board)
        players[0].HoleCards = [C(Rank.Three, Suit.Hearts), C(Rank.Four, Suit.Diamonds)];
        players[1].HoleCards = [C(Rank.Three, Suit.Spades), C(Rank.Four, Suit.Clubs)];

        var result = await _handler.ExecuteShowdownAsync(table);

        Assert.True(result.IsSuccess);

        // Both players should win (tie)
        Assert.True(result.TotalWinnings.ContainsKey(players[0].Id));
        Assert.True(result.TotalWinnings.ContainsKey(players[1].Id));
        Assert.Equal(50m, result.TotalWinnings[players[0].Id]);
        Assert.Equal(50m, result.TotalWinnings[players[1].Id]);
    }

    [Fact]
    public async Task ExecuteShowdownAsync_WithOddChips_AwardsRemainderToFirstFromButton()
    {
        var table = CreateTableWithPlayers(2);
        var hand = CreateShowdownHand(table, 101m); // Odd pot

        var players = table.Players.Values.OrderBy(p => p.SeatPosition).ToList();
        // Both players have same hand
        players[0].HoleCards = [C(Rank.Three, Suit.Hearts), C(Rank.Four, Suit.Diamonds)];
        players[1].HoleCards = [C(Rank.Three, Suit.Spades), C(Rank.Four, Suit.Clubs)];

        var result = await _handler.ExecuteShowdownAsync(table);

        Assert.True(result.IsSuccess);

        // First player from button gets the extra cent
        var winnings = result.TotalWinnings;
        Assert.True(winnings.Values.Sum() == 101m);
    }

    [Fact]
    public async Task ExecuteShowdownAsync_OddChipSplit_GoesToFirstToActFromButton()
    {
        // Scenario: Button at seat 5, winners at seats 3 and 7
        // Seat 7 is first to act (left of button), gets the odd chip
        // Pot: $15 → Seat 7 gets $8, Seat 3 gets $7
        var table = CreateTableWithSpecificSeats([3, 7], buttonPosition: 5);
        var hand = CreateShowdownHand(table, 15m);

        var playerAtSeat3 = table.Players.Values.First(p => p.SeatPosition == 3);
        var playerAtSeat7 = table.Players.Values.First(p => p.SeatPosition == 7);

        // Both players have identical hands (tie)
        playerAtSeat3.HoleCards = [C(Rank.Three, Suit.Hearts), C(Rank.Four, Suit.Diamonds)];
        playerAtSeat7.HoleCards = [C(Rank.Three, Suit.Spades), C(Rank.Four, Suit.Clubs)];

        var result = await _handler.ExecuteShowdownAsync(table);

        Assert.True(result.IsSuccess);

        // Seat 7 is left of button (seat 5), so seat 7 is first to act
        // First to act gets the odd chip
        Assert.Equal(8m, result.TotalWinnings[playerAtSeat7.Id]);
        Assert.Equal(7m, result.TotalWinnings[playerAtSeat3.Id]);
        Assert.Equal(15m, result.TotalWinnings.Values.Sum());
    }

    [Fact]
    public async Task ExecuteShowdownAsync_OddChipSplit_WrapsAroundTable()
    {
        // Scenario: Button at seat 8, winners at seats 2 and 9
        // Seat 9 is first to act (immediately left of button 8), gets the odd chip
        // Pot: $15 → Seat 9 gets $8, Seat 2 gets $7
        var table = CreateTableWithSpecificSeats([2, 9], buttonPosition: 8);
        var hand = CreateShowdownHand(table, 15m);

        var playerAtSeat2 = table.Players.Values.First(p => p.SeatPosition == 2);
        var playerAtSeat9 = table.Players.Values.First(p => p.SeatPosition == 9);

        // Identical hands
        playerAtSeat2.HoleCards = [C(Rank.Three, Suit.Hearts), C(Rank.Four, Suit.Diamonds)];
        playerAtSeat9.HoleCards = [C(Rank.Three, Suit.Spades), C(Rank.Four, Suit.Clubs)];

        var result = await _handler.ExecuteShowdownAsync(table);

        Assert.True(result.IsSuccess);

        // Seat 9 is left of button (seat 8), so seat 9 is first to act
        Assert.Equal(8m, result.TotalWinnings[playerAtSeat9.Id]);
        Assert.Equal(7m, result.TotalWinnings[playerAtSeat2.Id]);
    }

    [Fact]
    public async Task ExecuteShowdownAsync_ThreeWaySplitWithOddChip_FirstToActGetsExtra()
    {
        // Scenario: Button at seat 1, winners at seats 3, 6, 9
        // Seat 3 is first to act (first left of button 1)
        // Pot: $10 with 3 winners → $10 / 3 = $3.33, floor to $3.00 each = $9.00
        // Remainder: $1.00 goes to first to act (seat 3)
        // Seat 3 gets $4, seats 6 and 9 get $3 each
        var table = CreateTableWithSpecificSeats([3, 6, 9], buttonPosition: 1);
        var hand = CreateShowdownHand(table, 10m);

        var playerAtSeat3 = table.Players.Values.First(p => p.SeatPosition == 3);
        var playerAtSeat6 = table.Players.Values.First(p => p.SeatPosition == 6);
        var playerAtSeat9 = table.Players.Values.First(p => p.SeatPosition == 9);

        // Identical hands
        playerAtSeat3.HoleCards = [C(Rank.Three, Suit.Hearts), C(Rank.Four, Suit.Diamonds)];
        playerAtSeat6.HoleCards = [C(Rank.Three, Suit.Spades), C(Rank.Four, Suit.Clubs)];
        playerAtSeat9.HoleCards = [C(Rank.Three, Suit.Diamonds), C(Rank.Four, Suit.Hearts)];

        var result = await _handler.ExecuteShowdownAsync(table);

        Assert.True(result.IsSuccess);

        // Seat 3 is first to act from button 1, gets the extra $1
        Assert.Equal(4m, result.TotalWinnings[playerAtSeat3.Id]);
        Assert.Equal(3m, result.TotalWinnings[playerAtSeat6.Id]);
        Assert.Equal(3m, result.TotalWinnings[playerAtSeat9.Id]);
        Assert.Equal(10m, result.TotalWinnings.Values.Sum());
    }

    [Fact]
    public async Task ExecuteShowdownAsync_SidePotOddChip_FirstToActInSidePotGetsExtra()
    {
        // Scenario: 3 players, button at seat 1
        // Seats 3, 6, 9 - Player at seat 3 is all-in (short stack)
        // Main pot: $30 (all 3 eligible) - won by seat 3 (best hand)
        // Side pot: $15 (seats 6 and 9 eligible) - tied between 6 and 9
        // Side pot split: Seat 6 is first to act among side pot eligible players
        // Seat 6 gets $8, Seat 9 gets $7
        var table = CreateTableWithSpecificSeats([3, 6, 9], buttonPosition: 1);
        var hand = CreateShowdownHand(table, 30m);

        // Create side pot with only seats 6 and 9 eligible
        var playerAtSeat3 = table.Players.Values.First(p => p.SeatPosition == 3);
        var playerAtSeat6 = table.Players.Values.First(p => p.SeatPosition == 6);
        var playerAtSeat9 = table.Players.Values.First(p => p.SeatPosition == 9);

        var sidePot = Pot.CreateSidePot([playerAtSeat6.Id, playerAtSeat9.Id], 1);
        sidePot.Amount = 15m;
        hand.Pots.Add(sidePot);

        // Seat 3 has best hand (wins main pot)
        playerAtSeat3.HoleCards = [C(Rank.Ace, Suit.Hearts), C(Rank.Ace, Suit.Diamonds)];
        // Seats 6 and 9 have identical hands (tie for side pot)
        playerAtSeat6.HoleCards = [C(Rank.Three, Suit.Hearts), C(Rank.Four, Suit.Diamonds)];
        playerAtSeat9.HoleCards = [C(Rank.Three, Suit.Spades), C(Rank.Four, Suit.Clubs)];

        var result = await _handler.ExecuteShowdownAsync(table);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.PotAwards.Count);

        // Main pot: Seat 3 wins all $30
        Assert.Equal(30m, result.TotalWinnings[playerAtSeat3.Id]);

        // Side pot: Seat 6 is first to act (left of button), gets odd chip
        // Total for seat 6: $8, Total for seat 9: $7
        Assert.Equal(8m, result.TotalWinnings[playerAtSeat6.Id]);
        Assert.Equal(7m, result.TotalWinnings[playerAtSeat9.Id]);

        Assert.Equal(45m, result.TotalWinnings.Values.Sum()); // 30 + 15
    }

    [Fact]
    public async Task ExecuteShowdownAsync_MultiplePots_AwardsEachCorrectly()
    {
        var table = CreateTableWithPlayers(3);
        var hand = CreateShowdownHand(table, 60m);

        // Add a side pot
        var sidePot = Pot.CreateSidePot([table.Players.Keys.First()], 1);
        sidePot.Amount = 40m;
        sidePot.AddEligiblePlayer(table.Players.Keys.Skip(1).First());
        hand.Pots.Add(sidePot);

        var players = table.Players.Values.OrderBy(p => p.SeatPosition).ToList();
        // Player 1: Royal Flush (best)
        players[0].HoleCards = [C(Rank.Ace, Suit.Clubs), C(Rank.Queen, Suit.Clubs)];
        // Player 2: Pair
        players[1].HoleCards = [C(Rank.Five, Suit.Hearts), C(Rank.Five, Suit.Clubs)];
        // Player 3: High card
        players[2].HoleCards = [C(Rank.Nine, Suit.Hearts), C(Rank.Ten, Suit.Spades)];

        // Change community cards to include clubs for flush
        hand.CommunityCards.Clear();
        hand.CommunityCards.AddRange([
            C(Rank.Ten, Suit.Clubs),
            C(Rank.Jack, Suit.Clubs),
            C(Rank.King, Suit.Clubs),
            C(Rank.Two, Suit.Hearts),
            C(Rank.Three, Suit.Diamonds)
        ]);

        var result = await _handler.ExecuteShowdownAsync(table);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.PotAwards.Count);
    }

    #endregion

    #region GetShowOrder Tests

    [Fact]
    public void GetShowOrder_WithLastAggressor_AggressorShowsFirst()
    {
        var table = CreateTableWithPlayers(3);
        var hand = CreateShowdownHand(table);

        var players = table.Players.Values.OrderBy(p => p.SeatPosition).ToList();
        hand.LastAggressorId = players[1].Id; // Middle player was last aggressor

        // Set hole cards for all players
        foreach (var player in players)
        {
            player.HoleCards = [C(Rank.Ace, Suit.Hearts), C(Rank.King, Suit.Hearts)];
        }

        var showOrder = _handler.GetShowOrder(table);

        Assert.Equal(3, showOrder.Count);
        Assert.Equal(players[1].Id, showOrder[0]); // Last aggressor first
    }

    [Fact]
    public void GetShowOrder_NoAggressor_FirstToActShowsFirst()
    {
        var table = CreateTableWithPlayers(3);
        var hand = CreateShowdownHand(table);
        hand.LastAggressorId = null; // No aggressor (all checked)

        var players = table.Players.Values.OrderBy(p => p.SeatPosition).ToList();
        foreach (var player in players)
        {
            player.HoleCards = [C(Rank.Ace, Suit.Hearts), C(Rank.King, Suit.Hearts)];
        }

        var showOrder = _handler.GetShowOrder(table);

        Assert.Equal(3, showOrder.Count);
        // First to act is left of button (seat 2)
        Assert.Equal(players[1].Id, showOrder[0]);
    }

    [Fact]
    public void GetShowOrder_NoHand_ReturnsEmptyList()
    {
        var table = CreateTableWithPlayers(2);
        table.CurrentHand = null;

        var showOrder = _handler.GetShowOrder(table);

        Assert.Empty(showOrder);
    }

    #endregion

    #region RequestMuckAsync Tests

    [Fact]
    public async Task RequestMuckAsync_ValidPlayer_ReturnsTrue()
    {
        var table = CreateTableWithPlayers(2);
        var hand = CreateShowdownHand(table);

        var players = table.Players.Values.ToList();
        players[0].HoleCards = [C(Rank.Ace, Suit.Hearts), C(Rank.Ace, Suit.Diamonds)];
        players[1].HoleCards = [C(Rank.Two, Suit.Hearts), C(Rank.Three, Suit.Spades)];

        var result = await _handler.RequestMuckAsync(table, players[1].Id);

        Assert.True(result);
    }

    [Fact]
    public async Task RequestMuckAsync_NoHand_ReturnsFalse()
    {
        var table = CreateTableWithPlayers(2);
        table.CurrentHand = null;

        var result = await _handler.RequestMuckAsync(table, Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task RequestMuckAsync_PlayerNotInHand_ReturnsFalse()
    {
        var table = CreateTableWithPlayers(2);
        var hand = CreateShowdownHand(table);

        var players = table.Players.Values.ToList();
        players[0].Status = PlayerStatus.Folded; // Not in hand

        var result = await _handler.RequestMuckAsync(table, players[0].Id);

        Assert.False(result);
    }

    [Fact]
    public async Task RequestMuckAsync_NonexistentPlayer_ReturnsFalse()
    {
        var table = CreateTableWithPlayers(2);
        CreateShowdownHand(table);

        var result = await _handler.RequestMuckAsync(table, Guid.NewGuid());

        Assert.False(result);
    }

    #endregion

    #region Auto-Muck Tests

    [Fact]
    public async Task ExecuteShowdownAsync_InferiorHand_AutoMucks()
    {
        var table = CreateTableWithPlayers(2);
        var hand = CreateShowdownHand(table);
        hand.LastAggressorId = table.Players.Keys.First(); // First player shows first

        var players = table.Players.Values.OrderBy(p => p.SeatPosition).ToList();
        // Player 1: Royal Flush (shows first as aggressor)
        players[0].HoleCards = [C(Rank.Ace, Suit.Clubs), C(Rank.Queen, Suit.Clubs)];
        // Player 2: Low pair (should auto-muck)
        players[1].HoleCards = [C(Rank.Two, Suit.Hearts), C(Rank.Two, Suit.Spades)];

        // Modify community for flush
        hand.CommunityCards.Clear();
        hand.CommunityCards.AddRange([
            C(Rank.Ten, Suit.Clubs),
            C(Rank.Jack, Suit.Clubs),
            C(Rank.King, Suit.Clubs),
            C(Rank.Three, Suit.Hearts),
            C(Rank.Four, Suit.Diamonds)
        ]);

        var result = await _handler.ExecuteShowdownAsync(table);

        Assert.True(result.IsSuccess);

        var player2Result = result.PlayerResults.First(r => r.PlayerId == players[1].Id);
        Assert.False(player2Result.Showed);
        Assert.True(player2Result.AutoMucked);
    }

    [Fact]
    public async Task ExecuteShowdownAsync_RequestedMuck_HonorsRequest()
    {
        var table = CreateTableWithPlayers(2);
        var hand = CreateShowdownHand(table);
        hand.LastAggressorId = table.Players.Keys.First();

        var players = table.Players.Values.OrderBy(p => p.SeatPosition).ToList();
        players[0].HoleCards = [C(Rank.Ace, Suit.Hearts), C(Rank.Ace, Suit.Diamonds)];
        players[1].HoleCards = [C(Rank.Two, Suit.Hearts), C(Rank.Three, Suit.Spades)];

        // Request muck before showdown
        await _handler.RequestMuckAsync(table, players[1].Id);

        var result = await _handler.ExecuteShowdownAsync(table);

        Assert.True(result.IsSuccess);

        var player2Result = result.PlayerResults.First(r => r.PlayerId == players[1].Id);
        Assert.False(player2Result.Showed);
    }

    #endregion

    #region Event Recording Tests

    [Fact]
    public async Task ExecuteShowdownAsync_RecordsShowEvents()
    {
        var table = CreateTableWithPlayers(2);
        var hand = CreateShowdownHand(table);

        var players = table.Players.Values.ToList();
        players[0].HoleCards = [C(Rank.Ace, Suit.Hearts), C(Rank.Ace, Suit.Diamonds)];
        players[1].HoleCards = [C(Rank.King, Suit.Hearts), C(Rank.King, Suit.Diamonds)];

        await _handler.ExecuteShowdownAsync(table);

        // Verify events were recorded
        await _eventStore.Received().AppendAsync(
            Arg.Any<PlayerShowedCardsEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteShowdownAsync_RecordsPotAwardedEvents()
    {
        var table = CreateTableWithPlayers(2);
        var hand = CreateShowdownHand(table);

        var players = table.Players.Values.ToList();
        players[0].HoleCards = [C(Rank.Ace, Suit.Hearts), C(Rank.Ace, Suit.Diamonds)];
        players[1].HoleCards = [C(Rank.King, Suit.Hearts), C(Rank.King, Suit.Diamonds)];

        await _handler.ExecuteShowdownAsync(table);

        await _eventStore.Received().AppendAsync(
            Arg.Any<PotAwardedEvent>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Hand Description Tests

    [Fact]
    public async Task ExecuteShowdownAsync_PotAward_IncludesHandDescription()
    {
        var table = CreateTableWithPlayers(2);
        var hand = CreateShowdownHand(table);

        var players = table.Players.Values.ToList();
        // Player 1: Pair of Aces
        players[0].HoleCards = [C(Rank.Ace, Suit.Hearts), C(Rank.Ace, Suit.Diamonds)];
        players[1].HoleCards = [C(Rank.Three, Suit.Hearts), C(Rank.Four, Suit.Spades)];

        var result = await _handler.ExecuteShowdownAsync(table);

        Assert.True(result.IsSuccess);
        Assert.Single(result.PotAwards);

        var award = result.PotAwards[0];
        Assert.False(string.IsNullOrEmpty(award.WinningHandDescription));
    }

    [Fact]
    public async Task ExecuteShowdownAsync_WinnerResult_IncludesEvaluatedHand()
    {
        var table = CreateTableWithPlayers(2);
        var hand = CreateShowdownHand(table);

        var players = table.Players.Values.ToList();
        players[0].HoleCards = [C(Rank.Ace, Suit.Hearts), C(Rank.Ace, Suit.Diamonds)];
        players[1].HoleCards = [C(Rank.Three, Suit.Hearts), C(Rank.Four, Suit.Spades)];

        var result = await _handler.ExecuteShowdownAsync(table);

        var winnerResult = result.PlayerResults.First(r => r.AmountWon > 0);
        Assert.NotNull(winnerResult.EvaluatedHand);
        Assert.Equal(HandCategory.Pair, winnerResult.EvaluatedHand.Value.Category);
    }

    #endregion
}
