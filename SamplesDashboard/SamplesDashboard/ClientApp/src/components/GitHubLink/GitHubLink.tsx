// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { FontIcon, Link } from '@fluentui/react';

interface GetHubLinkProps {
  repoUrl: string;
  relativeUrl: string;
  text: string;
  icon?: string;
  iconClass?: string;
}

// Display a link to a GitHub page with an optional
// icon
export default function GetHubLink(props: GetHubLinkProps) {
  return (
    <Link
      href={`${props.repoUrl}/${props.relativeUrl}`}
      target='_blank'
      rel='noreferrer'
    >
      <span>
        {props.icon && (
          <FontIcon iconName={props.icon} className={props.iconClass} />
        )}
        {props.text}
      </span>
    </Link>
  );
}
