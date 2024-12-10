# Odds feed sdk

## Generate classes from xsd

```bash
git clone https://github.com/oddin-gg/oddsfeedschema.git
docker run --rm \
    -v "$$(pwd)/src/Oddin.OddsFeedSdk/Oddin.OddsFeedSdk/AMQP/Messages:/src/messages" \
    -v "$$(pwd)/oddsfeedschema:/src/oddsfeedschema" \
    -w "/src/oddsfeedschema/schema/feed" \
    mono /bin/sh -c "rm /src/messages/RollbackBetCancel.cs && xsd rollback_bet_cancel.xsd /c /o:/src/messages /n:Oddin.OddsFeedSdk.AMQP.Messages && mv /src/messages/rollback_bet_cancel.cs /src/messages/RollbackBetCancel.cs"
rm -rf oddsfeedschema
```

`rollback_bet_cancel` is used as an example, modify the script to use the xsd you need.

After generation the class needs to inherit from `FeedMessageModel`, implement missing properties.