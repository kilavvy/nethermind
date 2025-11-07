#!/bin/bash
# SPDX-FileCopyrightText: 2023 Demerzel Solutions Limited
# SPDX-License-Identifier: LGPL-3.0-only

set -euo pipefail

known_fails=$(cat ./nethermind/scripts/known-failing-hive-tests.txt)
# In some test suites this test is a client setup and in some it's a master test.
# So just ignore it.
launch_test='client launch (nethermind)'
should_pass=()
should_not_pass=()

for passed in "true" "false"; do
  tmp=()
  mapfile tmp < <(jq '.testCases
    | map_values(select(.summaryResult.pass == $p))
    | map(.name)
    | .[]' \
    --argjson p "$passed" -r $1)
  IFS=$'\n' results=($(sort -f <<<"${tmp[*]}")); unset IFS

  if [[ "$passed" == "true" ]]; then
    echo -e "\nPassed ${#results[@]}:\n"

    for each in "${results[@]}"; do
      echo -e "\033[0;32m\u2714\033[0m $each"

      if grep -Fqx "$each" <<< "$known_fails" && [[ "$each" != "$launch_test" ]]; then
        should_not_pass+=("$each")
      fi
    done
  else
    echo -e "\nFailed ${#results[@]}:\n"

    for each in "${results[@]}"; do
      if ! grep -Fqx "$each" <<< "$known_fails" && [[ "$each" != "$launch_test" ]]; then
        should_pass+=("$each")
        echo -e "\033[0;31m\u2716 $each\033[0m"
      else
        echo -e "\033[90m\u2716 $each\033[0m"
      fi
    done
  fi
done

(( ${#should_not_pass[@]} + ${#should_pass[@]} > 0 )) && exit 1
