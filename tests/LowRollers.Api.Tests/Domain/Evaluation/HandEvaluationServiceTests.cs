using LowRollers.Api.Domain.Evaluation;
using LowRollers.Api.Domain.Models;

namespace LowRollers.Api.Tests.Domain.Evaluation;

public class HandEvaluationServiceTests
{
    private readonly HandEvaluationService _service = new();

    #region Helper Methods

    private static Card C(Rank rank, Suit suit) => new(suit, rank);

    private static List<Card> CreateRoyalFlush() =>
    [
        C(Rank.Ace, Suit.Hearts),
        C(Rank.King, Suit.Hearts),
        C(Rank.Queen, Suit.Hearts),
        C(Rank.Jack, Suit.Hearts),
        C(Rank.Ten, Suit.Hearts)
    ];

    private static List<Card> CreateStraightFlush() =>
    [
        C(Rank.Nine, Suit.Spades),
        C(Rank.Eight, Suit.Spades),
        C(Rank.Seven, Suit.Spades),
        C(Rank.Six, Suit.Spades),
        C(Rank.Five, Suit.Spades)
    ];

    private static List<Card> CreateFourOfAKind() =>
    [
        C(Rank.King, Suit.Hearts),
        C(Rank.King, Suit.Diamonds),
        C(Rank.King, Suit.Clubs),
        C(Rank.King, Suit.Spades),
        C(Rank.Two, Suit.Hearts)
    ];

    private static List<Card> CreateFullHouse() =>
    [
        C(Rank.Jack, Suit.Hearts),
        C(Rank.Jack, Suit.Diamonds),
        C(Rank.Jack, Suit.Clubs),
        C(Rank.Seven, Suit.Hearts),
        C(Rank.Seven, Suit.Spades)
    ];

    private static List<Card> CreateFlush() =>
    [
        C(Rank.Ace, Suit.Diamonds),
        C(Rank.Ten, Suit.Diamonds),
        C(Rank.Seven, Suit.Diamonds),
        C(Rank.Four, Suit.Diamonds),
        C(Rank.Two, Suit.Diamonds)
    ];

    private static List<Card> CreateStraight() =>
    [
        C(Rank.Ten, Suit.Hearts),
        C(Rank.Nine, Suit.Diamonds),
        C(Rank.Eight, Suit.Clubs),
        C(Rank.Seven, Suit.Spades),
        C(Rank.Six, Suit.Hearts)
    ];

    private static List<Card> CreateThreeOfAKind() =>
    [
        C(Rank.Queen, Suit.Hearts),
        C(Rank.Queen, Suit.Diamonds),
        C(Rank.Queen, Suit.Clubs),
        C(Rank.Eight, Suit.Spades),
        C(Rank.Two, Suit.Hearts)
    ];

    private static List<Card> CreateTwoPair() =>
    [
        C(Rank.Nine, Suit.Hearts),
        C(Rank.Nine, Suit.Diamonds),
        C(Rank.Five, Suit.Clubs),
        C(Rank.Five, Suit.Spades),
        C(Rank.King, Suit.Hearts)
    ];

    private static List<Card> CreatePair() =>
    [
        C(Rank.Ace, Suit.Hearts),
        C(Rank.Ace, Suit.Diamonds),
        C(Rank.King, Suit.Clubs),
        C(Rank.Queen, Suit.Spades),
        C(Rank.Jack, Suit.Hearts)
    ];

    private static List<Card> CreateHighCard() =>
    [
        C(Rank.Ace, Suit.Hearts),
        C(Rank.King, Suit.Diamonds),
        C(Rank.Ten, Suit.Clubs),
        C(Rank.Five, Suit.Spades),
        C(Rank.Two, Suit.Hearts)
    ];

    private static List<Card> CreateWheelStraight() =>
    [
        C(Rank.Ace, Suit.Hearts),
        C(Rank.Two, Suit.Diamonds),
        C(Rank.Three, Suit.Clubs),
        C(Rank.Four, Suit.Spades),
        C(Rank.Five, Suit.Hearts)
    ];

    #endregion

    #region Hand Category Tests

    [Fact]
    public void Evaluate_RoyalFlush_ReturnsCorrectCategory()
    {
        var cards = CreateRoyalFlush();

        var result = _service.Evaluate(cards);

        Assert.Equal(HandCategory.RoyalFlush, result.Category);
    }

    [Fact]
    public void Evaluate_StraightFlush_ReturnsCorrectCategory()
    {
        var cards = CreateStraightFlush();

        var result = _service.Evaluate(cards);

        Assert.Equal(HandCategory.StraightFlush, result.Category);
    }

    [Fact]
    public void Evaluate_FourOfAKind_ReturnsCorrectCategory()
    {
        var cards = CreateFourOfAKind();

        var result = _service.Evaluate(cards);

        Assert.Equal(HandCategory.FourOfAKind, result.Category);
    }

    [Fact]
    public void Evaluate_FullHouse_ReturnsCorrectCategory()
    {
        var cards = CreateFullHouse();

        var result = _service.Evaluate(cards);

        Assert.Equal(HandCategory.FullHouse, result.Category);
    }

    [Fact]
    public void Evaluate_Flush_ReturnsCorrectCategory()
    {
        var cards = CreateFlush();

        var result = _service.Evaluate(cards);

        Assert.Equal(HandCategory.Flush, result.Category);
    }

    [Fact]
    public void Evaluate_Straight_ReturnsCorrectCategory()
    {
        var cards = CreateStraight();

        var result = _service.Evaluate(cards);

        Assert.Equal(HandCategory.Straight, result.Category);
    }

    [Fact]
    public void Evaluate_ThreeOfAKind_ReturnsCorrectCategory()
    {
        var cards = CreateThreeOfAKind();

        var result = _service.Evaluate(cards);

        Assert.Equal(HandCategory.ThreeOfAKind, result.Category);
    }

    [Fact]
    public void Evaluate_TwoPair_ReturnsCorrectCategory()
    {
        var cards = CreateTwoPair();

        var result = _service.Evaluate(cards);

        Assert.Equal(HandCategory.TwoPair, result.Category);
    }

    [Fact]
    public void Evaluate_Pair_ReturnsCorrectCategory()
    {
        var cards = CreatePair();

        var result = _service.Evaluate(cards);

        Assert.Equal(HandCategory.Pair, result.Category);
    }

    [Fact]
    public void Evaluate_HighCard_ReturnsCorrectCategory()
    {
        var cards = CreateHighCard();

        var result = _service.Evaluate(cards);

        Assert.Equal(HandCategory.HighCard, result.Category);
    }

    [Fact]
    public void Evaluate_WheelStraight_ReturnsCorrectCategory()
    {
        var cards = CreateWheelStraight();

        var result = _service.Evaluate(cards);

        Assert.Equal(HandCategory.Straight, result.Category);
    }

    #endregion

    #region Hand Ranking Comparison Tests

    [Fact]
    public void Evaluate_RoyalFlush_HasBetterRankingThanStraightFlush()
    {
        var royalFlush = _service.Evaluate(CreateRoyalFlush());
        var straightFlush = _service.Evaluate(CreateStraightFlush());

        Assert.True(royalFlush.Beats(straightFlush));
    }

    [Fact]
    public void Evaluate_StraightFlush_HasBetterRankingThanFourOfAKind()
    {
        var straightFlush = _service.Evaluate(CreateStraightFlush());
        var fourOfAKind = _service.Evaluate(CreateFourOfAKind());

        Assert.True(straightFlush.Beats(fourOfAKind));
    }

    [Fact]
    public void Evaluate_FourOfAKind_HasBetterRankingThanFullHouse()
    {
        var fourOfAKind = _service.Evaluate(CreateFourOfAKind());
        var fullHouse = _service.Evaluate(CreateFullHouse());

        Assert.True(fourOfAKind.Beats(fullHouse));
    }

    [Fact]
    public void Evaluate_FullHouse_HasBetterRankingThanFlush()
    {
        var fullHouse = _service.Evaluate(CreateFullHouse());
        var flush = _service.Evaluate(CreateFlush());

        Assert.True(fullHouse.Beats(flush));
    }

    [Fact]
    public void Evaluate_Flush_HasBetterRankingThanStraight()
    {
        var flush = _service.Evaluate(CreateFlush());
        var straight = _service.Evaluate(CreateStraight());

        Assert.True(flush.Beats(straight));
    }

    [Fact]
    public void Evaluate_Straight_HasBetterRankingThanThreeOfAKind()
    {
        var straight = _service.Evaluate(CreateStraight());
        var threeOfAKind = _service.Evaluate(CreateThreeOfAKind());

        Assert.True(straight.Beats(threeOfAKind));
    }

    [Fact]
    public void Evaluate_ThreeOfAKind_HasBetterRankingThanTwoPair()
    {
        var threeOfAKind = _service.Evaluate(CreateThreeOfAKind());
        var twoPair = _service.Evaluate(CreateTwoPair());

        Assert.True(threeOfAKind.Beats(twoPair));
    }

    [Fact]
    public void Evaluate_TwoPair_HasBetterRankingThanPair()
    {
        var twoPair = _service.Evaluate(CreateTwoPair());
        var pair = _service.Evaluate(CreatePair());

        Assert.True(twoPair.Beats(pair));
    }

    [Fact]
    public void Evaluate_Pair_HasBetterRankingThanHighCard()
    {
        var pair = _service.Evaluate(CreatePair());
        var highCard = _service.Evaluate(CreateHighCard());

        Assert.True(pair.Beats(highCard));
    }

    #endregion

    #region Kicker Comparison Tests (5-Card Hands)

    [Fact]
    public void Evaluate_TwoPairWithBetterKicker_Wins()
    {
        // A-A-K-K-Q vs A-A-K-K-J - Queen kicker beats Jack kicker
        var twoPairQueenKicker = new List<Card>
        {
            C(Rank.Ace, Suit.Hearts),
            C(Rank.Ace, Suit.Diamonds),
            C(Rank.King, Suit.Clubs),
            C(Rank.King, Suit.Spades),
            C(Rank.Queen, Suit.Hearts)
        };

        var twoPairJackKicker = new List<Card>
        {
            C(Rank.Ace, Suit.Clubs),
            C(Rank.Ace, Suit.Spades),
            C(Rank.King, Suit.Hearts),
            C(Rank.King, Suit.Diamonds),
            C(Rank.Jack, Suit.Clubs)
        };

        var queenKicker = _service.Evaluate(twoPairQueenKicker);
        var jackKicker = _service.Evaluate(twoPairJackKicker);

        Assert.Equal(HandCategory.TwoPair, queenKicker.Category);
        Assert.Equal(HandCategory.TwoPair, jackKicker.Category);
        Assert.True(queenKicker.Beats(jackKicker));
        Assert.False(jackKicker.Beats(queenKicker));
    }

    [Fact]
    public void Evaluate_TripsWithBetterKicker_Wins()
    {
        // Q-Q-Q-A-K vs Q-Q-Q-A-J - King second kicker beats Jack
        var tripsKingKicker = new List<Card>
        {
            C(Rank.Queen, Suit.Hearts),
            C(Rank.Queen, Suit.Diamonds),
            C(Rank.Queen, Suit.Clubs),
            C(Rank.Ace, Suit.Spades),
            C(Rank.King, Suit.Hearts)
        };

        var tripsJackKicker = new List<Card>
        {
            C(Rank.Queen, Suit.Spades),
            C(Rank.Queen, Suit.Hearts),
            C(Rank.Queen, Suit.Diamonds),
            C(Rank.Ace, Suit.Clubs),
            C(Rank.Jack, Suit.Hearts)
        };

        var kingKicker = _service.Evaluate(tripsKingKicker);
        var jackKicker = _service.Evaluate(tripsJackKicker);

        Assert.Equal(HandCategory.ThreeOfAKind, kingKicker.Category);
        Assert.Equal(HandCategory.ThreeOfAKind, jackKicker.Category);
        Assert.True(kingKicker.Beats(jackKicker));
    }

    [Fact]
    public void Evaluate_PairWithBetterThirdKicker_Wins()
    {
        // A-A-K-Q-J vs A-A-K-Q-9 - Jack third kicker beats Nine
        var pairJackKicker = new List<Card>
        {
            C(Rank.Ace, Suit.Hearts),
            C(Rank.Ace, Suit.Diamonds),
            C(Rank.King, Suit.Clubs),
            C(Rank.Queen, Suit.Spades),
            C(Rank.Jack, Suit.Hearts)
        };

        var pairNineKicker = new List<Card>
        {
            C(Rank.Ace, Suit.Clubs),
            C(Rank.Ace, Suit.Spades),
            C(Rank.King, Suit.Hearts),
            C(Rank.Queen, Suit.Diamonds),
            C(Rank.Nine, Suit.Clubs)
        };

        var jackKicker = _service.Evaluate(pairJackKicker);
        var nineKicker = _service.Evaluate(pairNineKicker);

        Assert.Equal(HandCategory.Pair, jackKicker.Category);
        Assert.Equal(HandCategory.Pair, nineKicker.Category);
        Assert.True(jackKicker.Beats(nineKicker));
    }

    [Fact]
    public void Evaluate_HighCardWithBetterFifthCard_Wins()
    {
        // A-K-Q-J-9 vs A-K-Q-J-8 - Nine fifth card beats Eight
        var highCardNine = new List<Card>
        {
            C(Rank.Ace, Suit.Hearts),
            C(Rank.King, Suit.Diamonds),
            C(Rank.Queen, Suit.Clubs),
            C(Rank.Jack, Suit.Spades),
            C(Rank.Nine, Suit.Hearts)
        };

        var highCardEight = new List<Card>
        {
            C(Rank.Ace, Suit.Clubs),
            C(Rank.King, Suit.Spades),
            C(Rank.Queen, Suit.Hearts),
            C(Rank.Jack, Suit.Diamonds),
            C(Rank.Eight, Suit.Clubs)
        };

        var nineHigh = _service.Evaluate(highCardNine);
        var eightHigh = _service.Evaluate(highCardEight);

        Assert.Equal(HandCategory.HighCard, nineHigh.Category);
        Assert.Equal(HandCategory.HighCard, eightHigh.Category);
        Assert.True(nineHigh.Beats(eightHigh));
    }

    [Fact]
    public void Evaluate_FullHouseWithHigherTrips_Wins()
    {
        // Q-Q-Q-7-7 vs J-J-J-A-A - Queens full beats Jacks full (trips rank matters, not pair)
        var queensFull = new List<Card>
        {
            C(Rank.Queen, Suit.Hearts),
            C(Rank.Queen, Suit.Diamonds),
            C(Rank.Queen, Suit.Clubs),
            C(Rank.Seven, Suit.Spades),
            C(Rank.Seven, Suit.Hearts)
        };

        var jacksFull = new List<Card>
        {
            C(Rank.Jack, Suit.Hearts),
            C(Rank.Jack, Suit.Diamonds),
            C(Rank.Jack, Suit.Clubs),
            C(Rank.Ace, Suit.Spades),
            C(Rank.Ace, Suit.Hearts)
        };

        var queensFullHand = _service.Evaluate(queensFull);
        var jacksFullHand = _service.Evaluate(jacksFull);

        Assert.Equal(HandCategory.FullHouse, queensFullHand.Category);
        Assert.Equal(HandCategory.FullHouse, jacksFullHand.Category);
        Assert.True(queensFullHand.Beats(jacksFullHand));
    }

    [Fact]
    public void Evaluate_FlushWithHigherFifthCard_Wins()
    {
        // A-K-Q-J-9 flush vs A-K-Q-J-8 flush - Nine beats Eight
        var flushNineHigh = new List<Card>
        {
            C(Rank.Ace, Suit.Hearts),
            C(Rank.King, Suit.Hearts),
            C(Rank.Queen, Suit.Hearts),
            C(Rank.Jack, Suit.Hearts),
            C(Rank.Nine, Suit.Hearts)
        };

        var flushEightHigh = new List<Card>
        {
            C(Rank.Ace, Suit.Diamonds),
            C(Rank.King, Suit.Diamonds),
            C(Rank.Queen, Suit.Diamonds),
            C(Rank.Jack, Suit.Diamonds),
            C(Rank.Eight, Suit.Diamonds)
        };

        var nineFlush = _service.Evaluate(flushNineHigh);
        var eightFlush = _service.Evaluate(flushEightHigh);

        Assert.Equal(HandCategory.Flush, nineFlush.Category);
        Assert.Equal(HandCategory.Flush, eightFlush.Category);
        Assert.True(nineFlush.Beats(eightFlush));
    }

    [Fact]
    public void Evaluate_QuadsWithBetterKicker_Wins()
    {
        // K-K-K-K-A vs K-K-K-K-Q - Ace kicker beats Queen kicker
        var quadsAceKicker = new List<Card>
        {
            C(Rank.King, Suit.Hearts),
            C(Rank.King, Suit.Diamonds),
            C(Rank.King, Suit.Clubs),
            C(Rank.King, Suit.Spades),
            C(Rank.Ace, Suit.Hearts)
        };

        var quadsQueenKicker = new List<Card>
        {
            C(Rank.King, Suit.Hearts),
            C(Rank.King, Suit.Diamonds),
            C(Rank.King, Suit.Clubs),
            C(Rank.King, Suit.Spades),
            C(Rank.Queen, Suit.Clubs)
        };

        var aceKicker = _service.Evaluate(quadsAceKicker);
        var queenKicker = _service.Evaluate(quadsQueenKicker);

        Assert.Equal(HandCategory.FourOfAKind, aceKicker.Category);
        Assert.Equal(HandCategory.FourOfAKind, queenKicker.Category);
        Assert.True(aceKicker.Beats(queenKicker));
    }

    [Fact]
    public void Evaluate_SameHandSameKickers_IsTie()
    {
        // A-A-K-Q-J vs A-A-K-Q-J (different suits) - true tie for split pot
        var hand1 = new List<Card>
        {
            C(Rank.Ace, Suit.Hearts),
            C(Rank.Ace, Suit.Diamonds),
            C(Rank.King, Suit.Clubs),
            C(Rank.Queen, Suit.Spades),
            C(Rank.Jack, Suit.Hearts)
        };

        var hand2 = new List<Card>
        {
            C(Rank.Ace, Suit.Clubs),
            C(Rank.Ace, Suit.Spades),
            C(Rank.King, Suit.Hearts),
            C(Rank.Queen, Suit.Diamonds),
            C(Rank.Jack, Suit.Clubs)
        };

        var evaluated1 = _service.Evaluate(hand1);
        var evaluated2 = _service.Evaluate(hand2);

        Assert.Equal(evaluated1.Ranking, evaluated2.Ranking);
        Assert.False(evaluated1.Beats(evaluated2));
        Assert.False(evaluated2.Beats(evaluated1));
    }

    [Fact]
    public void DetermineWinners_KickerDeterminesWinner_ReturnsSingleWinner()
    {
        // Three players with same two pair, different kickers
        var queenKicker = _service.Evaluate(new List<Card>
        {
            C(Rank.Ace, Suit.Hearts), C(Rank.Ace, Suit.Diamonds),
            C(Rank.King, Suit.Clubs), C(Rank.King, Suit.Spades),
            C(Rank.Queen, Suit.Hearts)
        });

        var jackKicker = _service.Evaluate(new List<Card>
        {
            C(Rank.Ace, Suit.Clubs), C(Rank.Ace, Suit.Spades),
            C(Rank.King, Suit.Hearts), C(Rank.King, Suit.Diamonds),
            C(Rank.Jack, Suit.Clubs)
        });

        var tenKicker = _service.Evaluate(new List<Card>
        {
            C(Rank.Ace, Suit.Hearts), C(Rank.Ace, Suit.Clubs),
            C(Rank.King, Suit.Spades), C(Rank.King, Suit.Hearts),
            C(Rank.Ten, Suit.Diamonds)
        });

        var winners = _service.DetermineWinners([queenKicker, jackKicker, tenKicker]);

        Assert.Single(winners);
        Assert.Equal(queenKicker.Ranking, winners[0].Ranking);
    }

    #endregion

    #region Seven Card Evaluation Tests

    [Fact]
    public void Evaluate_SevenCards_FindsBestFiveCardHand()
    {
        // Royal flush hidden in 7 cards
        var cards = new List<Card>
        {
            C(Rank.Ace, Suit.Hearts),
            C(Rank.King, Suit.Hearts),
            C(Rank.Queen, Suit.Hearts),
            C(Rank.Jack, Suit.Hearts),
            C(Rank.Ten, Suit.Hearts),
            C(Rank.Two, Suit.Clubs),      // Extra card
            C(Rank.Three, Suit.Diamonds)  // Extra card
        };

        var result = _service.Evaluate(cards);

        Assert.Equal(HandCategory.RoyalFlush, result.Category);
    }

    [Fact]
    public void Evaluate_WithHoleCardsAndCommunity_EvaluatesCorrectly()
    {
        var holeCards = new List<Card>
        {
            C(Rank.Ace, Suit.Hearts),
            C(Rank.King, Suit.Hearts)
        };

        var communityCards = new List<Card>
        {
            C(Rank.Queen, Suit.Hearts),
            C(Rank.Jack, Suit.Hearts),
            C(Rank.Ten, Suit.Hearts)
        };

        var result = _service.Evaluate(holeCards, communityCards);

        Assert.Equal(HandCategory.RoyalFlush, result.Category);
    }

    [Fact]
    public void Evaluate_SevenCards_SelectsBestKickers()
    {
        // Hole: A-K, Board: A-Q-8-5-2 -> Best hand is A-A-K-Q-8 (pair of aces)
        var holeCards = new List<Card>
        {
            C(Rank.Ace, Suit.Hearts),
            C(Rank.King, Suit.Diamonds)
        };

        var communityCards = new List<Card>
        {
            C(Rank.Ace, Suit.Clubs),
            C(Rank.Queen, Suit.Spades),
            C(Rank.Eight, Suit.Hearts),
            C(Rank.Five, Suit.Diamonds),
            C(Rank.Two, Suit.Clubs)
        };

        var result = _service.Evaluate(holeCards, communityCards);

        Assert.Equal(HandCategory.Pair, result.Category);

        // Verify this beats a weaker kicker hand
        // Hole: A-7, same board -> A-A-Q-8-7
        var weakerHole = new List<Card>
        {
            C(Rank.Ace, Suit.Spades),
            C(Rank.Seven, Suit.Hearts)
        };
        var weakerResult = _service.Evaluate(weakerHole, communityCards);

        Assert.True(result.Beats(weakerResult));
    }

    [Fact]
    public void Evaluate_SevenCards_BoardPlaysWhenBetter()
    {
        // Hole: 2-3, Board: A-K-Q-J-T (broadway straight on board)
        // Best hand is the board straight - hole cards don't play
        var holeCards = new List<Card>
        {
            C(Rank.Two, Suit.Hearts),
            C(Rank.Three, Suit.Diamonds)
        };

        var communityCards = new List<Card>
        {
            C(Rank.Ace, Suit.Clubs),
            C(Rank.King, Suit.Spades),
            C(Rank.Queen, Suit.Hearts),
            C(Rank.Jack, Suit.Diamonds),
            C(Rank.Ten, Suit.Clubs)
        };

        var result = _service.Evaluate(holeCards, communityCards);

        Assert.Equal(HandCategory.Straight, result.Category);

        // Another player with same worthless hole cards ties (board plays for both)
        var otherHole = new List<Card>
        {
            C(Rank.Four, Suit.Spades),
            C(Rank.Five, Suit.Clubs)
        };
        var otherResult = _service.Evaluate(otherHole, communityCards);

        Assert.Equal(result.Ranking, otherResult.Ranking);
    }

    [Fact]
    public void Evaluate_SevenCards_PartialHoleCardsPlay()
    {
        // Board: A-Q-J-T-8 (gap at K and 9)
        // Hole: K-2 -> A-K-Q-J-T straight (Broadway, only K plays)
        // Hole: 9-3 -> Q-J-T-9-8 straight (Queen-high, only 9 plays)
        var communityCards = new List<Card>
        {
            C(Rank.Ace, Suit.Clubs),
            C(Rank.Queen, Suit.Spades),
            C(Rank.Jack, Suit.Hearts),
            C(Rank.Ten, Suit.Diamonds),
            C(Rank.Eight, Suit.Clubs)
        };

        // Player 1: K-2 -> Broadway straight (A-K-Q-J-T)
        var holeCards = new List<Card>
        {
            C(Rank.Two, Suit.Hearts),
            C(Rank.King, Suit.Diamonds)
        };

        var result = _service.Evaluate(holeCards, communityCards);
        Assert.Equal(HandCategory.Straight, result.Category);

        // Player 2: 9-3 -> Queen-high straight (Q-J-T-9-8)
        var worseHole = new List<Card>
        {
            C(Rank.Nine, Suit.Spades),
            C(Rank.Three, Suit.Clubs)
        };
        var worseResult = _service.Evaluate(worseHole, communityCards);

        Assert.Equal(HandCategory.Straight, worseResult.Category);
        Assert.True(result.Beats(worseResult));
    }

    [Fact]
    public void Evaluate_SevenCards_KickerCompetitionFromSevenCards()
    {
        // Two players with pair of aces, different kickers from 7-card hands
        // Player 1: A-K hole, Board: A-Q-J-8-3 -> A-A-K-Q-J
        // Player 2: A-7 hole, same board -> A-A-Q-J-8
        var board = new List<Card>
        {
            C(Rank.Ace, Suit.Clubs),
            C(Rank.Queen, Suit.Spades),
            C(Rank.Jack, Suit.Hearts),
            C(Rank.Eight, Suit.Diamonds),
            C(Rank.Three, Suit.Clubs)
        };

        var player1Hole = new List<Card>
        {
            C(Rank.Ace, Suit.Hearts),
            C(Rank.King, Suit.Diamonds)
        };

        var player2Hole = new List<Card>
        {
            C(Rank.Ace, Suit.Spades),
            C(Rank.Seven, Suit.Hearts)
        };

        var player1Hand = _service.Evaluate(player1Hole, board);
        var player2Hand = _service.Evaluate(player2Hole, board);

        Assert.Equal(HandCategory.Pair, player1Hand.Category);
        Assert.Equal(HandCategory.Pair, player2Hand.Category);
        Assert.True(player1Hand.Beats(player2Hand));

        var winners = _service.DetermineWinners([player1Hand, player2Hand]);
        Assert.Single(winners);
    }

    [Fact]
    public void Evaluate_SevenCards_BoardQuadsKickerDeterminesWinner()
    {
        // Board has quads, player with best 5th card wins
        // Board: K-K-K-K-5
        // Player 1: A-2 hole -> K-K-K-K-A
        // Player 2: Q-2 hole -> K-K-K-K-Q
        var board = new List<Card>
        {
            C(Rank.King, Suit.Hearts),
            C(Rank.King, Suit.Diamonds),
            C(Rank.King, Suit.Clubs),
            C(Rank.King, Suit.Spades),
            C(Rank.Five, Suit.Hearts)
        };

        var player1Hole = new List<Card>
        {
            C(Rank.Ace, Suit.Hearts),
            C(Rank.Two, Suit.Diamonds)
        };

        var player2Hole = new List<Card>
        {
            C(Rank.Queen, Suit.Clubs),
            C(Rank.Two, Suit.Spades)
        };

        var player1Hand = _service.Evaluate(player1Hole, board);
        var player2Hand = _service.Evaluate(player2Hole, board);

        Assert.Equal(HandCategory.FourOfAKind, player1Hand.Category);
        Assert.Equal(HandCategory.FourOfAKind, player2Hand.Category);
        Assert.True(player1Hand.Beats(player2Hand));
    }

    [Fact]
    public void Evaluate_SevenCards_BoardQuadsTieWhenBoardKickerPlays()
    {
        // Board: K-K-K-K-A - Ace on board is best kicker
        // Player 1: 2-3 hole -> K-K-K-K-A (board plays)
        // Player 2: 4-5 hole -> K-K-K-K-A (board plays)
        // Result: Tie
        var board = new List<Card>
        {
            C(Rank.King, Suit.Hearts),
            C(Rank.King, Suit.Diamonds),
            C(Rank.King, Suit.Clubs),
            C(Rank.King, Suit.Spades),
            C(Rank.Ace, Suit.Hearts)
        };

        var player1Hole = new List<Card>
        {
            C(Rank.Two, Suit.Hearts),
            C(Rank.Three, Suit.Diamonds)
        };

        var player2Hole = new List<Card>
        {
            C(Rank.Four, Suit.Clubs),
            C(Rank.Five, Suit.Spades)
        };

        var player1Hand = _service.Evaluate(player1Hole, board);
        var player2Hand = _service.Evaluate(player2Hole, board);

        Assert.Equal(HandCategory.FourOfAKind, player1Hand.Category);
        Assert.Equal(player1Hand.Ranking, player2Hand.Ranking);

        var winners = _service.DetermineWinners([player1Hand, player2Hand]);
        Assert.Equal(2, winners.Count);
    }

    [Fact]
    public void Evaluate_SevenCards_FlushSelectsBestFiveOfSuit()
    {
        // Hole: Ah-2h, Board: Kh-Qh-Jh-8h-3c
        // 6 hearts available, best 5 is A-K-Q-J-8
        var holeCards = new List<Card>
        {
            C(Rank.Ace, Suit.Hearts),
            C(Rank.Two, Suit.Hearts)
        };

        var communityCards = new List<Card>
        {
            C(Rank.King, Suit.Hearts),
            C(Rank.Queen, Suit.Hearts),
            C(Rank.Jack, Suit.Hearts),
            C(Rank.Eight, Suit.Hearts),
            C(Rank.Three, Suit.Clubs)
        };

        var result = _service.Evaluate(holeCards, communityCards);

        Assert.Equal(HandCategory.Flush, result.Category);

        // Player with worse flush hole cards loses
        // Hole: 9h-2h -> K-Q-J-9-8 flush
        var worseHole = new List<Card>
        {
            C(Rank.Nine, Suit.Hearts),
            C(Rank.Two, Suit.Spades)  // Non-heart
        };
        var worseResult = _service.Evaluate(worseHole, communityCards);

        Assert.Equal(HandCategory.Flush, worseResult.Category);
        Assert.True(result.Beats(worseResult));
    }

    #endregion

    #region Winner Determination Tests

    [Fact]
    public void DetermineWinners_SingleWinner_ReturnsOneHand()
    {
        var royalFlush = _service.Evaluate(CreateRoyalFlush());
        var straightFlush = _service.Evaluate(CreateStraightFlush());
        var fullHouse = _service.Evaluate(CreateFullHouse());

        var winners = _service.DetermineWinners([royalFlush, straightFlush, fullHouse]);

        Assert.Single(winners);
        Assert.Equal(HandCategory.RoyalFlush, winners[0].Category);
    }

    [Fact]
    public void DetermineWinners_TiedHands_ReturnsMultipleWinners()
    {
        // Two identical pairs (both pair of aces with same kickers)
        var pair1 = new List<Card>
        {
            C(Rank.Ace, Suit.Hearts),
            C(Rank.Ace, Suit.Diamonds),
            C(Rank.King, Suit.Clubs),
            C(Rank.Queen, Suit.Spades),
            C(Rank.Jack, Suit.Hearts)
        };

        var pair2 = new List<Card>
        {
            C(Rank.Ace, Suit.Clubs),
            C(Rank.Ace, Suit.Spades),
            C(Rank.King, Suit.Hearts),
            C(Rank.Queen, Suit.Diamonds),
            C(Rank.Jack, Suit.Clubs)
        };

        var evaluated1 = _service.Evaluate(pair1);
        var evaluated2 = _service.Evaluate(pair2);

        var winners = _service.DetermineWinners([evaluated1, evaluated2]);

        Assert.Equal(2, winners.Count);
    }

    [Fact]
    public void DetermineWinners_EmptyList_ReturnsEmptyList()
    {
        var winners = _service.DetermineWinners([]);

        Assert.Empty(winners);
    }

    #endregion

    #region RankHands Tests

    [Fact]
    public void RankHands_OrdersByRanking_BestFirst()
    {
        var highCard = _service.Evaluate(CreateHighCard());
        var pair = _service.Evaluate(CreatePair());
        var royalFlush = _service.Evaluate(CreateRoyalFlush());

        var ranked = _service.RankHands([highCard, pair, royalFlush]);

        Assert.Equal(HandCategory.RoyalFlush, ranked[0].Category);
        Assert.Equal(HandCategory.Pair, ranked[1].Category);
        Assert.Equal(HandCategory.HighCard, ranked[2].Category);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void Evaluate_TooFewCards_ThrowsArgumentException()
    {
        var cards = new List<Card>
        {
            C(Rank.Ace, Suit.Hearts),
            C(Rank.King, Suit.Hearts),
            C(Rank.Queen, Suit.Hearts),
            C(Rank.Jack, Suit.Hearts)
        };

        Assert.Throws<ArgumentException>(() => _service.Evaluate(cards));
    }

    [Fact]
    public void Evaluate_TooManyCards_ThrowsArgumentException()
    {
        var cards = new List<Card>
        {
            C(Rank.Ace, Suit.Hearts),
            C(Rank.King, Suit.Hearts),
            C(Rank.Queen, Suit.Hearts),
            C(Rank.Jack, Suit.Hearts),
            C(Rank.Ten, Suit.Hearts),
            C(Rank.Nine, Suit.Hearts),
            C(Rank.Eight, Suit.Hearts),
            C(Rank.Seven, Suit.Hearts)
        };

        Assert.Throws<ArgumentException>(() => _service.Evaluate(cards));
    }

    [Fact]
    public void Evaluate_WrongHoleCardCount_ThrowsArgumentException()
    {
        var holeCards = new List<Card>
        {
            C(Rank.Ace, Suit.Hearts)  // Only 1 hole card
        };

        var communityCards = new List<Card>
        {
            C(Rank.Queen, Suit.Hearts),
            C(Rank.Jack, Suit.Hearts),
            C(Rank.Ten, Suit.Hearts)
        };

        Assert.Throws<ArgumentException>(() => _service.Evaluate(holeCards, communityCards));
    }

    [Fact]
    public void Evaluate_WrongCommunityCardCount_ThrowsArgumentException()
    {
        var holeCards = new List<Card>
        {
            C(Rank.Ace, Suit.Hearts),
            C(Rank.King, Suit.Hearts)
        };

        var communityCards = new List<Card>
        {
            C(Rank.Queen, Suit.Hearts),
            C(Rank.Jack, Suit.Hearts)  // Only 2 community cards
        };

        Assert.Throws<ArgumentException>(() => _service.Evaluate(holeCards, communityCards));
    }

    #endregion

    #region Description Tests

    [Fact]
    public void Evaluate_ReturnsNonEmptyDescription()
    {
        var cards = CreateRoyalFlush();

        var result = _service.Evaluate(cards);

        Assert.False(string.IsNullOrEmpty(result.Description));
    }

    [Fact]
    public void Evaluate_DescriptionContainsHandInfo()
    {
        var cards = CreateFullHouse();

        var result = _service.Evaluate(cards);

        // Description should mention the hand details (e.g., "Jacks Full over Sevens")
        // The library uses descriptive names like "X Full over Y" for full houses
        Assert.True(
            result.Description.Contains("Full", StringComparison.OrdinalIgnoreCase) ||
            result.Description.Contains("Jacks", StringComparison.OrdinalIgnoreCase),
            $"Expected description to contain hand info, but got: {result.Description}");
    }

    #endregion

    #region GetRanking Tests

    [Fact]
    public void GetRanking_ReturnsConsistentValue()
    {
        var cards = CreateFlush();

        var ranking1 = _service.GetRanking(cards);
        var ranking2 = _service.GetRanking(cards);

        Assert.Equal(ranking1, ranking2);
    }

    [Fact]
    public void GetRanking_BetterHandHasLowerRanking()
    {
        var royalFlushRanking = _service.GetRanking(CreateRoyalFlush());
        var highCardRanking = _service.GetRanking(CreateHighCard());

        Assert.True(royalFlushRanking < highCardRanking);
    }

    #endregion
}
